[CmdletBinding()]
param (
    [string]$RestId = "55045",
    [string]$Locale = "ru"
)

$ErrorActionPreference = "Stop"

Write-Host "==> Fetching Bistro 24 menu data (Restaurant ID: $RestId)..." -ForegroundColor Cyan

$url = "https://bistro-24.com/api/payload/get?internal=true&currentLocale=$Locale"

try {
    $wc = New-Object System.Net.WebClient
    $wc.Encoding = [System.Text.Encoding]::UTF8
    $wc.Headers.Add("Accept", "application/json, text/plain, */*")
    $wc.Headers.Add("nuxtinternalreq", "true")
    $wc.Headers.Add("Referer", "https://bistro-24.com/")
    $wc.Headers.Add("Cookie", "cookiesAccepted=1; restId=$RestId")
    $wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)")

    $rawJson = $wc.DownloadString($url)
    $response = $rawJson | ConvertFrom-Json
}
catch {
    Write-Error "Failed to fetch menu from Bistro 24: $($_.Exception.Message)"
    exit 1
}

if (-not $response -or -not $response.dishes) {
    Write-Error "Failed to extract dishes from Bistro 24 API response."
    exit 1
}

Write-Host "Raw items received: $($response.dishes.Count)" -ForegroundColor Green

# Filter out non-food and clean names
$cleanDishes = [System.Collections.Generic.List[PSObject]]::new()
$cleanNames = [System.Collections.Generic.List[string]]::new()

foreach ($dish in $response.dishes) {
    $rawName = [string]$dish.name
    if ([string]::IsNullOrWhiteSpace($rawName)) { continue }
    
    # Filter delivery and service items
    if ($rawName -match 'доставка|до двери|пакет' -or $dish.categoryName -match 'доставка') {
        continue
    }

    # Clean name (remove leading "д " and normalize spaces)
    $cleanName = ($rawName -replace '^д\s+', '') -replace '\s+', ' '
    $cleanName = $cleanName.Trim()
    if ([string]::IsNullOrWhiteSpace($cleanName)) { continue }

    # Photo URL (prefer webp, then mediumSquare, large, original)
    $photoUrl = $null
    if ($dish.photo) {
        if ($dish.photo.webp) { $photoUrl = $dish.photo.webp }
        elseif ($dish.photo.mediumSquare) { $photoUrl = $dish.photo.mediumSquare }
        elseif ($dish.photo.large) { $photoUrl = $dish.photo.large }
        elseif ($dish.photo.original) { $photoUrl = $dish.photo.original }
    }

    # Weight formatted
    $weightStr = $null
    if ($dish.weight) {
        $unit = if ($dish.weightUnit -eq 'gram') { 'г' } elseif ($dish.weightUnit -eq 'kilogram') { 'кг' } else { $dish.weightUnit }
        $weightStr = "$($dish.weight) $unit"
    }

    $dishObj = [PSCustomObject]@{
        id          = $dish.id
        name        = $cleanName
        category    = [string]$dish.categoryName
        categoryId  = $dish.categoryId
        price       = $dish.price
        weight      = $weightStr
        description = $dish.description
        photo       = $photoUrl
    }

    $cleanDishes.Add($dishObj)
    if (-not $cleanNames.Contains($cleanName)) {
        $cleanNames.Add($cleanName)
    }
}

Write-Host "Clean food items processed: $($cleanDishes.Count)" -ForegroundColor Green

$scriptDir = $PSScriptRoot
if (-not $scriptDir) { $scriptDir = (Get-Location).Path }

$utf8NoBom = [System.Text.UTF8Encoding]::new($false)

# 1. Write menu.json (rich structured data)
$jsonPath = Join-Path -Path $scriptDir -ChildPath "menu.json"
$jsonContent = $cleanDishes | ConvertTo-Json -Depth 4
[System.IO.File]::WriteAllText($jsonPath, $jsonContent, $utf8NoBom)
Write-Host "  -> Wrote menu.json" -ForegroundColor Gray

# 2. Write menu.js (for browser usage with backward compatibility)
$jsPath = Join-Path -Path $scriptDir -ChildPath "menu.js"
$escapedNames = $cleanNames | ForEach-Object {
    $escaped = $_ -replace '\\', '\\' -replace '"', '\"'
    "`"$escaped`""
}
$jsContent = "const MENU_DATA = " + ($cleanDishes | ConvertTo-Json -Depth 4 -Compress) + ";`n" + `
             "const MENU_ITEMS = [$($escapedNames -join ', ')];`n"
[System.IO.File]::WriteAllText($jsPath, $jsContent, $utf8NoBom)
Write-Host "  -> Wrote menu.js" -ForegroundColor Gray

# 3. Write menu.txt (plain text list)
$txtPath = Join-Path -Path $scriptDir -ChildPath "menu.txt"
[System.IO.File]::WriteAllLines($txtPath, $cleanNames, $utf8NoBom)
Write-Host "  -> Wrote menu.txt" -ForegroundColor Gray

$categoriesCount = ($cleanDishes | Select-Object -ExpandProperty category -Unique | Measure-Object).Count
Write-Host "==> Menu update completed successfully! ($($cleanNames.Count) unique dishes across $categoriesCount categories)" -ForegroundColor Cyan
