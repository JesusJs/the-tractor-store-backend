using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Sales.Domain; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    // Aquí registrarás tus DbSets de dominio más adelante
    // public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelCreatingBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Todo lo de catálogo irá bajo su propio esquema en Postgres para no mezclar tablas
        modelBuilder.HasDefaultSchema("sales");
        
        // Aquí aplicarás los mapeos de Fluent API de tus agregados
    }
}