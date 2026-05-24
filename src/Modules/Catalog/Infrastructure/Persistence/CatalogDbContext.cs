using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Domain;
using TractorEcommerce.Modules.Catalog.Domain.Entities; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Store> Stores => Set<Store>();

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
            builder.Property(v => v.Stock).IsRequired();
        });

        // Configuración de Store
        modelBuilder.Entity<Store>(builder =>
        {
            builder.ToTable("stores");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).HasMaxLength(50);
            builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
            builder.Property(s => s.Address).HasMaxLength(250).IsRequired();
            builder.Property(s => s.City).HasMaxLength(100).IsRequired();
            builder.Property(s => s.Image).HasMaxLength(500);
        });

        // Sembrado de Datos Iniciales (Seed Data)
        modelBuilder.Entity<Product>().HasData(
            new { Id = "tx-001", Name = "Autonomous Titan", Brand = "TractorCorp", Price = 85000m, Image = "https://placehold.co/600x400/png?text=Autonomous+Titan", Description = "Premium autonomous driving tractor.", Category = "autonomous", EnginePower = "240 HP", Highlights = new[] { "GPS Guided Autonomous System", "240 HP High-Performance Power", "Dynamic Torque Control & Field Optimization", "Full Warranty & physical maintenance support included" } },
            new { Id = "tx-002", Name = "Classic Vintage 1950", Brand = "HeritageIron", Price = 45000m, Image = "https://placehold.co/600x400/png?text=Classic+Vintage", Description = "Beautifully restored post-war utility tractor.", Category = "classics", EnginePower = "45 HP", Highlights = new[] { "Standard High-Performance Power", "Full Warranty & physical maintenance support included" } }
        );

        modelBuilder.Entity<ProductVariant>().HasData(
            new { Sku = "TX-001-GPS", ProductId = "tx-001", name = "GPS Edition", Stock = 8, product_id = "tx-001" },
            new { Sku = "TX-001-AI", ProductId = "tx-001", name = "AI Edition", Stock = 3, product_id = "tx-001" },
            new { Sku = "TX-CLS-01", ProductId = "tx-002", name = "Standard Edition", Stock = 0, product_id = "tx-002" }
        );

        modelBuilder.Entity<Store>().HasData(
            new { Id = "store-central", Name = "Central Headquarters", Address = "Av. de la Maquinaria 404", City = "Madrid", Image = "https://placehold.co/300x200" },
            new { Id = "store-north", Name = "North Hub", Address = "Industrial Route 66, Km 12", City = "Burgos", Image = "https://placehold.co/300x200" }
        );
    }
}