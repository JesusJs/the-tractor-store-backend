using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Domain; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    // Aquí registrarás tus DbSets de dominio más adelante
    // public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí va la configuración de tus entidades (Tractores, etc.)
    }
}