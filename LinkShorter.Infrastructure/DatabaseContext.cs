using LinkShorter.Core.Entities;
using LinkShorter.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace LinkShorter.Infrastructure;

public class DatabaseContext : DbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="DatabaseContext"/>.
    /// </summary>
    /// <param name="options"><see cref="DbContextOptions"/>.</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
    
    public DbSet<ShortLink> ShortLinks { get; set; } = null!;

    public DbSet<BarcodeInfo> BarcodeInfos { get; set; } = null!;

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new ShortLinkConfiguration())
            .ApplyConfiguration(new BarcodeInfoConfiguration());
    }
}