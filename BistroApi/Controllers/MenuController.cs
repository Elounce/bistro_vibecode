using BistroApi.Data;
using BistroApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistroApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly BistroDbContext _db;
    private readonly ILogger<MenuController> _logger;

    public MenuController(BistroDbContext db, ILogger<MenuController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Получить все элементы меню с фильтрацией по категории, доступности и поисковому запросу.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll(
        [FromQuery] Category? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? onlyAvailable = null)
    {
        IQueryable<MenuItem> query = _db.MenuItems.AsNoTracking();

        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        if (onlyAvailable.HasValue && onlyAvailable.Value)
        {
            query = query.Where(x => x.IsAvailable);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(term) || (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        var items = await query
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return Ok(items.Select(MapToDto));
    }

    /// <summary>
    /// Получить случайный набор блюд для вращения колеса фортуны.
    /// </summary>
    [HttpGet("random")]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetRandom(
        [FromQuery] int count = 10,
        [FromQuery] Category? category = null)
    {
        if (count < 1) count = 1;
        if (count > 50) count = 50;

        IQueryable<MenuItem> query = _db.MenuItems.AsNoTracking().Where(x => x.IsAvailable);
        if (category.HasValue)
        {
            query = query.Where(x => x.Category == category.Value);
        }

        var allMatching = await query.ToListAsync();
        if (allMatching.Count == 0)
        {
            return Ok(Enumerable.Empty<MenuItemDto>());
        }

        var randomSample = allMatching
            .OrderBy(_ => Random.Shared.Next())
            .Take(count)
            .Select(MapToDto);

        return Ok(randomSample);
    }

    /// <summary>
    /// Получить блюдо по ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuItemDto>> GetById(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { error = $"Menu item with ID {id} not found." });
        }

        return Ok(MapToDto(item));
    }

    /// <summary>
    /// Создать новое блюдо в меню.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MenuItemDto>> Create([FromBody] CreateMenuItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = new MenuItem
        {
            Name = dto.Name.Trim(),
            Category = dto.Category,
            Price = dto.Price,
            Weight = dto.Weight?.Trim(),
            Description = dto.Description?.Trim(),
            PhotoUrl = dto.PhotoUrl?.Trim(),
            IsAvailable = dto.IsAvailable,
            CreatedAt = DateTime.UtcNow
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    /// <summary>
    /// Обновить существующее блюдо в меню.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MenuItemDto>> Update(int id, [FromBody] UpdateMenuItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var item = await _db.MenuItems.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { error = $"Menu item with ID {id} not found." });
        }

        item.Name = dto.Name.Trim();
        item.Category = dto.Category;
        item.Price = dto.Price;
        item.Weight = dto.Weight?.Trim();
        item.Description = dto.Description?.Trim();
        item.PhotoUrl = dto.PhotoUrl?.Trim();
        item.IsAvailable = dto.IsAvailable;

        await _db.SaveChangesAsync();

        return Ok(MapToDto(item));
    }

    /// <summary>
    /// Удалить блюдо из меню.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null)
        {
            return NotFound(new { error = $"Menu item with ID {id} not found." });
        }

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static MenuItemDto MapToDto(MenuItem item) =>
        new(
            item.Id,
            item.Name,
            item.Category,
            item.Category.ToString(),
            item.Price,
            item.Weight,
            item.Description,
            item.PhotoUrl,
            item.IsAvailable
        );
}
