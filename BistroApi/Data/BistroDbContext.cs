using BistroApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BistroApi.Data;

public class BistroDbContext : DbContext
{
    public BistroDbContext(DbContextOptions<BistroDbContext> options) : base(options)
    {
    }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Category).HasConversion<int>();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Weight).HasMaxLength(50);
            entity.Property(e => e.PhotoUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsAvailable);
        });
    }
}
