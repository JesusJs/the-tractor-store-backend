using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Sales.Domain; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    // Aquí registrarás tus DbSets de dominio más adelante
    // public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aquí va la configuración de tus entidades (Tractores, etc.)
    }
}