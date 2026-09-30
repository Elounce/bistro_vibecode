using System.Text.Json;
using System.Text.Json.Serialization;
using BistroApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BistroApi.Data;

public static class DbInitializer
{
    private class JsonMenuItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("price")]
        public JsonElement Price { get; set; }

        [JsonPropertyName("weight")]
        public string? Weight { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("photo")]
        public string? Photo { get; set; }
    }

    public static async Task InitializeAsync(BistroDbContext db, IWebHostEnvironment env, ILogger logger)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.MenuItems.AnyAsync())
        {
            return;
        }

        logger.LogInformation("MenuItems table is empty. Attempting to seed from menu.json...");

        // Look for menu.json in ContentRootPath, root, or BaseDirectory
        var possiblePaths = new[]
        {
            Path.Combine(env.ContentRootPath, "menu.json"),
            Path.Combine(env.ContentRootPath, "..", "menu.json"),
            Path.Combine(AppContext.BaseDirectory, "menu.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "menu.json")
        };

        string? jsonPath = possiblePaths.FirstOrDefault(File.Exists);
        if (jsonPath is null)
        {
            logger.LogWarning("menu.json not found in search paths. Seeding demo items fallback.");
            await SeedDemoItemsAsync(db);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            var dishes = JsonSerializer.Deserialize<List<JsonMenuItem>>(json);

            if (dishes is null || dishes.Count == 0)
            {
                await SeedDemoItemsAsync(db);
                return;
            }

            var items = new List<MenuItem>();
            foreach (var dish in dishes)
            {
                if (string.IsNullOrWhiteSpace(dish.Name)) continue;

                decimal? price = null;
                if (dish.Price.ValueKind == JsonValueKind.Number && dish.Price.TryGetDecimal(out var pNum))
                {
                    price = pNum;
                }
                else if (dish.Price.ValueKind == JsonValueKind.String && decimal.TryParse(dish.Price.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var pStr))
                {
                    price = pStr;
                }

                items.Add(new MenuItem
                {
                    Name = dish.Name.Trim(),
                    Category = MapCategory(dish.Category, dish.Name),
                    Price = price,
                    Weight = dish.Weight,
                    Description = dish.Description,
                    PhotoUrl = dish.Photo,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.MenuItems.AddRangeAsync(items);
            await db.SaveChangesAsync();
            logger.LogInformation("Successfully seeded {Count} menu items from {Path}", items.Count, jsonPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed menu items from menu.json. Seeding demo items fallback.");
            await SeedDemoItemsAsync(db);
        }
    }

    private static Category MapCategory(string? rawCategory, string name)
    {
        var cat = (rawCategory ?? string.Empty).ToLowerInvariant();
        var lowerName = name.ToLowerInvariant();

        if (cat.Contains("пицц") || lowerName.Contains("пицца") || lowerName.Contains("хачапури")) return Category.Pizza;
        if (cat.Contains("ролл") || lowerName.Contains("ролл") || lowerName.Contains("сет")) return Category.Rolls;
        if (cat.Contains("шаурм") || lowerName.Contains("шаурма")) return Category.Shawarma;
        if (cat.Contains("фастфуд") || lowerName.Contains("бургер") || lowerName.Contains("хот-дог")) return Category.Burgers;
        if (cat.Contains("напит") || lowerName.Contains("cola") || lowerName.Contains("сок") || lowerName.Contains("вода")) return Category.Drinks;
        if (cat.Contains("соус") || lowerName.Contains("соус")) return Category.Sauces;
        if (cat.Contains("горяч") || lowerName.Contains("удон") || lowerName.Contains("том ям")) return Category.HotDishes;
        if (cat.Contains("салат") || lowerName.Contains("салат") || lowerName.Contains("боул")) return Category.SaladsAndBowls;

        return Category.Other;
    }

    private static async Task SeedDemoItemsAsync(BistroDbContext db)
    {
        var demo = new[]
        {
            new MenuItem { Name = "Пицца Пепперони", Category = Category.Pizza, Price = 350, IsAvailable = true },
            new MenuItem { Name = "Ролл Филадельфия", Category = Category.Rolls, Price = 350, IsAvailable = true },
            new MenuItem { Name = "Бургер Мясной", Category = Category.Burgers, Price = 300, IsAvailable = true },
            new MenuItem { Name = "Шаурма в Лаваше", Category = Category.Shawarma, Price = 150, IsAvailable = true }
        };
        await db.MenuItems.AddRangeAsync(demo);
        await db.SaveChangesAsync();
    }
}
