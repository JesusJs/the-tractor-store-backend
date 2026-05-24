using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Domain;
using TractorEcommerce.Modules.Catalog.Domain.Entities; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Requisito de arquitectura: Aislamiento por esquema de base de datos
        modelBuilder.HasDefaultSchema("catalog");

        // Configuración de Product
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Brand).HasMaxLength(100).IsRequired();

            // Decimales de alta precisión obligatorios para precios de maquinaria pesada
            builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(p => p.Image).HasMaxLength(500);
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.EnginePower).HasMaxLength(50);

            // Mapeo de la lista de strings (Highlights) como una columna JSONB o Array Nativo de Postgres
            builder.Property(p => p.Highlights)
                .HasColumnType("text[]");

            // Relación 1-a-Muchos con sus variantes (SKUs)
            builder.HasMany(p => p.Variants)
                .WithOne()
                .HasForeignKey("product_id") // Columna Sombra (Shadow Property)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuración de ProductVariant
        modelBuilder.Entity<ProductVariant>(builder =>
        {
            builder.ToTable("product_variants");
            builder.HasKey(v => v.Sku); // Tu SKU actúa como Llave Primaria Natural

            builder.Property(v => v.Sku).HasMaxLength(50);
            //builder.Property(v => v.Name).HasMaxLength(100).IsRequired();
            builder.Property(v => v.Stock).IsRequired();
        });
    }
}