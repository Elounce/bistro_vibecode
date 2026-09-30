using System.ComponentModel.DataAnnotations;

namespace BistroApi.Models;

public record MenuItemDto(
    int Id,
    string Name,
    Category Category,
    string CategoryName,
    decimal? Price,
    string? Description,
    bool IsAvailable
);

public record CreateMenuItemDto(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters")]
    string Name,
    
    Category Category = Category.Other,
    
    [Range(0, 100000, ErrorMessage = "Price must be non-negative")]
    decimal? Price = null,
    
    string? Description = null,
    
    bool IsAvailable = true
);

public record UpdateMenuItemDto(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters")]
    string Name,
    
    Category Category = Category.Other,
    
    [Range(0, 100000, ErrorMessage = "Price must be non-negative")]
    decimal? Price = null,
    
    string? Description = null,
    
    bool IsAvailable = true
);
