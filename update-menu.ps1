$webClient = New-Object System.Net.WebClient
$webClient.Encoding = [System.Text.Encoding]::UTF8
$webClient.Headers.Add("Accept", "*/*")
$webClient.Headers.Add("nuxtinternalreq", "true")
$webClient.Headers.Add("Referer", "https://bistro-24.com/")
$webClient.Headers.Add("Cookie", "cookiesAccepted=1; basketId=6a6217ab447f71a83bbe3862; restId=55045")

$utf8String = $webClient.DownloadString("https://bistro-24.com/api/payload/get?internal=true&currentLocale=ru")
$data = $utf8String | ConvertFrom-Json
$names = $data.dishes.name

$items = $names | ForEach-Object {
    $escaped = ($_ -replace '"', '\"')
    "`"$escaped`""
}

$jsArray = "const MENU_ITEMS = [$($items -join ', ')];"
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText((Join-Path -Path $PSScriptRoot -ChildPath "menu.js"), $jsArray, $utf8NoBom)

Write-Output "menu.js generated with $($names.Count) items"
