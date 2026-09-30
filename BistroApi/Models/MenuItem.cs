namespace BistroApi.Models;

public class MenuItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public Category Category { get; set; } = Category.Other;
    public decimal? Price { get; set; }
    public string? Weight { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
