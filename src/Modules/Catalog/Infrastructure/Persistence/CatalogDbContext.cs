using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Catalog.Domain;
using TractorEcommerce.Modules.Catalog.Domain.Entities; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
    public DbSet<Category> Category => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        // ---------- Category ----------
        modelBuilder.Entity<Category>(builder =>
        {
            builder.ToTable("category");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
            builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
            //builder.HasIndex(c => c.Slug).IsUnique();
        });

        // ---------- Product ----------
        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code).HasMaxLength(20).IsRequired();
            builder.HasIndex(p => p.Code).IsUnique();

            builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
            builder.Property(p => p.Brand).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.ImageUrl).HasMaxLength(500);
            builder.Property(p => p.EnginePower).HasMaxLength(50);

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Metadata.FindNavigation(nameof(Product.product_variants))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(p => p.product_variants)
                .WithOne()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- ProductVariant ----------
        modelBuilder.Entity<ProductVariant>(builder =>
        {
            builder.ToTable("product_variants"); // según tu tabla recomendada: catalog_variants
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Sku).HasMaxLength(50).IsRequired();
            builder.HasIndex(v => v.Sku).IsUnique(); // [oficial] sku único

            builder.Property(v => v.Name).HasMaxLength(150).IsRequired();
            builder.Property(v => v.Price).HasPrecision(10, 2).IsRequired();

            builder.Property(v => v.Highlights)
                .HasColumnType("text[]");
        });

        // ---------- Store ----------
        modelBuilder.Entity<Store>(builder =>
        {
            builder.ToTable("stores");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
            builder.Property(s => s.Address).HasMaxLength(250).IsRequired();
            builder.Property(s => s.City).HasMaxLength(100).IsRequired();
            builder.Property(s => s.ImageUrl).HasMaxLength(500);
        });

        SeedData(modelBuilder);
    }
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // ---------- Categorías ----------
        var categoryAutonomous = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var categoryClassic = Guid.Parse("11111111-0000-0000-0000-000000000002");

        modelBuilder.Entity<Category>().HasData(
            new { Id = categoryAutonomous, Name = "Autonomous", Slug = "autonomous" },
            new { Id = categoryClassic, Name = "Classic", Slug = "classic" }
        );

        // ---------- Tiendas ----------
        modelBuilder.Entity<Store>().HasData(
            new { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), Name = "Aurora Flagship Store", Address = "Astronaut Way 1", City = "Arlington", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/store/200/store-1.webp" },
            new { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), Name = "Big Micro Machines", Address = "Broadway 2", City = "Burlington", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/store/200/store-2.webp" },
            new { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), Name = "Central Mall", Address = "Clown Street 3", City = "Cryo", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/store/200/store-3.webp" },
            new { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), Name = "Downtown Model Store", Address = "Duck Street 4", City = "Davenport", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/store/200/store-4.webp" }
        );

        // ---------- Productos (GUIDs fijos por producto) ----------
        var pAU04 = Guid.Parse("33333333-0000-0000-0000-000000000001");
        var pCL08 = Guid.Parse("33333333-0000-0000-0000-000000000002");
        var pCL13 = Guid.Parse("33333333-0000-0000-0000-000000000003");
        var pCL15 = Guid.Parse("33333333-0000-0000-0000-000000000004");
        var pCL01 = Guid.Parse("33333333-0000-0000-0000-000000000005");
        var pAU08 = Guid.Parse("33333333-0000-0000-0000-000000000006");
        var pAU02 = Guid.Parse("33333333-0000-0000-0000-000000000007");
        var pAU07 = Guid.Parse("33333333-0000-0000-0000-000000000008");
        var pAU05 = Guid.Parse("33333333-0000-0000-0000-000000000009");
        var pCL12 = Guid.Parse("33333333-0000-0000-0000-000000000010");
        var pCL11 = Guid.Parse("33333333-0000-0000-0000-000000000011");
        var pCL09 = Guid.Parse("33333333-0000-0000-0000-000000000012");
        var pCL07 = Guid.Parse("33333333-0000-0000-0000-000000000013");
        var pCL06 = Guid.Parse("33333333-0000-0000-0000-000000000014");
        var pCL05 = Guid.Parse("33333333-0000-0000-0000-000000000015");
        var pCL02 = Guid.Parse("33333333-0000-0000-0000-000000000016");
        var pCL10 = Guid.Parse("33333333-0000-0000-0000-000000000017");
        var pCL03 = Guid.Parse("33333333-0000-0000-0000-000000000018");
        var pCL14 = Guid.Parse("33333333-0000-0000-0000-000000000019");
        var pCL04 = Guid.Parse("33333333-0000-0000-0000-000000000020");
        var pAU06 = Guid.Parse("33333333-0000-0000-0000-000000000021");
        var pAU03 = Guid.Parse("33333333-0000-0000-0000-000000000022");
        var pAU01 = Guid.Parse("33333333-0000-0000-0000-000000000023");

        modelBuilder.Entity<Product>().HasData(
            new { Id = pAU04, Code = "AU-04", CategoryId = categoryAutonomous, Name = "Sapphire Sunworker 460R", Brand = "TractorStore Autonomous", Description = "Unidad autónoma de alta gama impulsada por paneles solares integrados y un banco de baterías Sapphire Core. Diseñado para optimizar ciclos de cultivo continuos en grandes extensiones.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-04-RD.webp", EnginePower = "460 HP" },
            new { Id = pCL08, Code = "CL-08", CategoryId = categoryClassic, Name = "Holland Hamster", Brand = "TractorStore Classic", Description = "Tractor utilitario compacto de alta maniobrabilidad. Su diseño optimizado le permite trabajar eficientemente tanto en huertos cerrados como en invernaderos tecnificados.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-08-GR.webp", EnginePower = "95 HP" },
            new { Id = pCL13, Code = "CL-13", CategoryId = categoryClassic, Name = "Rapid Racer", Brand = "TractorStore Classic", Description = "La combinación perfecta entre velocidad de transporte terrestre y fuerza de toma de fuerza (PTO). Ideal para operaciones mixtas que requieren desplazamientos constantes.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-13-BL.webp", EnginePower = "180 HP" },
            new { Id = pCL15, Code = "CL-15", CategoryId = categoryClassic, Name = "Fieldmaster Classic", Brand = "TractorStore Classic", Description = "El caballo de batalla tradicional de la marca. Mecánica 100% analógica de fácil mantenimiento, construida para resistir las condiciones climáticas más hostiles del entorno.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-15-PI.webp", EnginePower = "210 HP" },
            new { Id = pCL01, Code = "CL-01", CategoryId = categoryClassic, Name = "Heritage Workhorse", Brand = "TractorStore Classic", Description = "Maquinaria pesada inspirada en los diseños industriales clásicos pero con un tren motriz modernizado. Fiabilidad estructural garantizada para labranza profunda.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-01-GR.webp", EnginePower = "165 HP" },
            new { Id = pAU08, Code = "AU-08", CategoryId = categoryAutonomous, Name = "Field Pioneer", Brand = "TractorStore Autonomous", Description = "Tractor robótico diseñado para reconocimiento inicial, preparación de camas de siembra y análisis de suelo en tiempo real mediante sensores integrados.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-08-WH.webp", EnginePower = "130 HP" },
            new { Id = pAU02, Code = "AU-02", CategoryId = categoryAutonomous, Name = "SmartFarm Titan", Brand = "TractorStore Autonomous", Description = "Unidad de alta potencia totalmente automatizada capaz de coordinar flotas de implementos dependientes mediante telemetría machine-to-machine.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-02-OG.webp", EnginePower = "380 HP" },
            new { Id = pAU07, Code = "AU-07", CategoryId = categoryAutonomous, Name = "Verde Voyager", Brand = "TractorStore Autonomous", Description = "Especializado en operaciones de cultivos ecológicos y mantenimiento de cobertura vegetal sin intervención humana directa. Silencioso y eficiente.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-07-MT.webp", EnginePower = "120 HP" },
            new { Id = pAU05, Code = "AU-05", CategoryId = categoryAutonomous, Name = "EcoGrow Crop Commander", Brand = "TractorStore Autonomous", Description = "Diseñado específicamente para la pulverización selectiva y dosificación de precisión. Reduce drásticamente el uso de insumos químicos mediante análisis de imagen.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-05-ZH.webp", EnginePower = "150 HP" },
            new { Id = pCL12, Code = "CL-12", CategoryId = categoryClassic, Name = "Celerity Cruiser", Brand = "TractorStore Classic", Description = "Tractor ágil ideal para tareas diarias de logística interna en la finca, transporte de forraje y remolques medianos.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-12-BL.webp", EnginePower = "110 HP" },
            new { Id = pCL11, Code = "CL-11", CategoryId = categoryClassic, Name = "Scandinavia Sower", Brand = "TractorStore Classic", Description = "Preparado con aislamiento térmico reforzado de fábrica y sistemas de precalentamiento de motor para trabajar en los inviernos y terrenos más exigentes.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-11-SK.webp", EnginePower = "140 HP" },
            new { Id = pCL09, Code = "CL-09", CategoryId = categoryClassic, Name = "TerraFirma Veneto", Brand = "TractorStore Classic", Description = "Un clásico del diseño europeo, configurado con un centro de gravedad bajo especial para viñedos y cultivos en laderas o terrenos escarpados.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-09-BL.webp", EnginePower = "100 HP" },
            new { Id = pCL07, Code = "CL-07", CategoryId = categoryClassic, Name = "Greenland Rover", Brand = "TractorStore Classic", Description = "Enfocado en pasturas y ganadería. Su sistema hidráulico trasero tiene una excelente relación de levante para acoplar segadoras y rotoempacadoras.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-07-GR.webp", EnginePower = "125 HP" },
            new { Id = pCL06, Code = "CL-06", CategoryId = categoryClassic, Name = "Danamark Steadfast", Brand = "TractorStore Classic", Description = "Construcción sólida y duradera con enfoque en la eficiencia del operador a largo plazo. Un equipo confiable año tras año.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-06-MT.webp", EnginePower = "135 HP" },
            new { Id = pCL05, Code = "CL-05", CategoryId = categoryClassic, Name = "Countryside Commander", Brand = "TractorStore Classic", Description = "Ideal para medianos productores que buscan la fuerza de un tractor grande sin perder la versatilidad de un chasis estándar.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-05-PT.webp", EnginePower = "150 HP" },
            new { Id = pCL02, Code = "CL-02", CategoryId = categoryClassic, Name = "Falcon Crest Farm", Brand = "TractorStore Classic", Description = "Diseñado para tareas agrícolas generales y operaciones ganaderas. Cuenta con un sistema hidráulico modular altamente configurable.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-02-BL.webp", EnginePower = "115 HP" },
            new { Id = pCL10, Code = "CL-10", CategoryId = categoryClassic, Name = "Global Gallant", Brand = "TractorStore Classic", Description = "Configuración versátil estandarizada para mercados globales. Fácil operación y alta disponibilidad de componentes de recambio.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-10-SD.webp", EnginePower = "120 HP" },
            new { Id = pCL03, Code = "CL-03", CategoryId = categoryClassic, Name = "Falcon Crest Work", Brand = "TractorStore Classic", Description = "Variación enfocada al trabajo pesado en granja, cargadores frontales y movimiento de tierras ligero dentro del predio.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-03-GR.webp", EnginePower = "110 HP" },
            new { Id = pCL14, Code = "CL-14", CategoryId = categoryClassic, Name = "Caribbean Cruiser", Brand = "TractorStore Classic", Description = "Diseño con protección anticorrosiva especial para zonas tropicales de alta humedad o plantaciones costeras de caña y arroz.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-14-GR.webp", EnginePower = "105 HP" },
            new { Id = pCL04, Code = "CL-04", CategoryId = categoryClassic, Name = "Broadfield Majestic", Brand = "TractorStore Classic", Description = "Tractor compacto y económico de bajo costo operativo, perfecto para pequeñas parcelas familiares o como soporte secundario.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-04-BL.webp", EnginePower = "85 HP" },
            new { Id = pAU06, Code = "AU-06", CategoryId = categoryAutonomous, Name = "FarmFleet Sovereign", Brand = "TractorStore Autonomous", Description = "Módulo autónomo especializado en patrullaje, mapeo de salud de cultivos mediante cámaras RGB e infrarrojas e índices NDVI.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-06-CZ.webp", EnginePower = "90 HP" },
            new { Id = pAU03, Code = "AU-03", CategoryId = categoryAutonomous, Name = "FutureHarvest Navigator", Brand = "TractorStore Autonomous", Description = "Pequeño rover autónomo multitarea, excelente para pasar entre hileras estrechas ejecutando desmalezado mecánico localizado.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-03-TQ.webp", EnginePower = "75 HP" },
            new { Id = pAU01, Code = "AU-01", CategoryId = categoryAutonomous, Name = "TerraFirma AutoCultivator T-300", Brand = "TractorStore Autonomous", Description = "Micro-unidad autónoma de entrada orientada a la automatización de tareas repetitivas a escala micro-agrícola o parcelas de prueba.", ImageUrl = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-01-SI.webp", EnginePower = "50 HP" }
        );

        // ---------- Variantes ----------
        // OJO: en tu seed original solo 5 productos tenían variante/SKU asignado.
        // Los otros 17 productos quedaron sin variante — te lo marco abajo para que decidas.
        modelBuilder.Entity<ProductVariant>().HasData(
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), ProductId = pAU04, Sku = "AU04-STD", Name = "Standard Sapphire Core", Price = 8500.00m, Highlights = new[] { "Alimentación fotovoltaica avanzada", "Mapeo topográfico inteligente", "Autonomía extendida 48h" } },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), ProductId = pCL08, Sku = "CL08-STD", Name = "Standard Wheels", Price = 7750.00m, Highlights = new[] { "Chasis ultracompacto", "Radio de giro cero", "Bajo consumo de combustible" } },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000003"), ProductId = pCL13, Sku = "CL13-FAST", Name = "High Speed Axle Kit", Price = 7500.00m, Highlights = new[] { "Transmisión Syncro-Fast", "Suspensión de cabina neumática", "Velocidad máxima optimizada" } },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000004"), ProductId = pCL15, Sku = "CL15-HD", Name = "Heavy Duty Iron Pack", Price = 6200.00m, Highlights = new[] { "Motor diésel de aspiración natural", "Cero componentes electrónicos críticos", "Tracción integral bloqueable" } },
            new { Id = Guid.Parse("44444444-0000-0000-0000-000000000005"), ProductId = pAU02, Sku = "AU02-XL", Name = "Dual Antenna Pro Pack", Price = 4000.00m, Highlights = new[] { "Procesamiento IA perimetral", "Doble antena satelital", "Sistema hidráulico inteligente" } }
        );

        // Stock (3, 8, 2, 5, 0) de estos 5 SKUs va a InventoryDbContext, no aquí.
    }
}

