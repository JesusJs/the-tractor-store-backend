using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Domain; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    // Aquí registrarás tus DbSets de dominio más adelante
    // public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelCreatingBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Todo lo de catálogo irá bajo su propio esquema en Postgres para no mezclar tablas
        modelBuilder.HasDefaultSchema("catalog");
        
        // Aquí aplicarás los mapeos de Fluent API de tus agregados
    }
}