using System.ComponentModel.DataAnnotations;

namespace BistroApi.Models;

public record MenuItemDto(
    int Id,
    string Name,
    Category Category,
    string CategoryName,
    decimal? Price,
    string? Weight,
    string? Description,
    string? PhotoUrl,
    bool IsAvailable
);

public record CreateMenuItemDto(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters")]
    string Name,
    
    Category Category = Category.Other,
    
    [Range(0, 100000, ErrorMessage = "Price must be non-negative")]
    decimal? Price = null,
    
    [StringLength(50, ErrorMessage = "Weight must not exceed 50 characters")]
    string? Weight = null,
    
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    string? Description = null,
    
    [Url(ErrorMessage = "PhotoUrl must be a valid URL")]
    string? PhotoUrl = null,
    
    bool IsAvailable = true
);

public record UpdateMenuItemDto(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters")]
    string Name,
    
    Category Category = Category.Other,
    
    [Range(0, 100000, ErrorMessage = "Price must be non-negative")]
    decimal? Price = null,
    
    [StringLength(50, ErrorMessage = "Weight must not exceed 50 characters")]
    string? Weight = null,
    
    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters")]
    string? Description = null,
    
    [Url(ErrorMessage = "PhotoUrl must be a valid URL")]
    string? PhotoUrl = null,
    
    bool IsAvailable = true
);
