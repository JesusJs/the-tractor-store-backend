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
        modelBuilder.Entity<Store>().HasData(
            new { Id = "store-aurora", Name = "Aurora Flagship Store", Address = "Astronaut Way 1", City = "Arlington", Image = "https://blueprint.the-tractor.store/cdn/img/store/200/store-1.webp" },
            new { Id = "store-big-micro", Name = "Big Micro Machines", Address = "Broadway 2", City = "Burlington", Image = "https://blueprint.the-tractor.store/cdn/img/store/200/store-2.webp" },
            new { Id = "store-central", Name = "Central Mall", Address = "Clown Street 3", City = "Cryo", Image = "https://blueprint.the-tractor.store/cdn/img/store/200/store-3.webp" },
            new { Id = "store-downtown", Name = "Downtown Model Store", Address = "Duck Street 4", City = "Davenport", Image = "https://blueprint.the-tractor.store/cdn/img/store/200/store-4.webp" }
        );

        // 2. Catálogo de Productos (Products)
        modelBuilder.Entity<Product>().HasData(
            new
            {
                Id = "AU-04",
                Name = "Sapphire Sunworker 460R",
                Brand = "TractorStore Autonomous",
                Price = 8500.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-04-RD.webp",
                Description = "Unidad autónoma de alta gama impulsada por paneles solares integrados y un banco de baterías Sapphire Core. Diseñado para optimizar ciclos de cultivo continuos en grandes extensiones.",
                Category = "autonomous",
                EnginePower = "460 HP",
                Highlights = new[] { "Alimentación fotovoltaica avanzada", "Mapeo topográfico inteligente", "Autonomía extendida 48h" }
            },
            new
            {
                Id = "CL-08",
                Name = "Holland Hamster",
                Brand = "TractorStore Classic",
                Price = 7750.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-08-GR.webp",
                Description = "Tractor utilitario compacto de alta maniobrabilidad. Su diseño optimizado le permite trabajar eficientemente tanto en huertos cerrados como en invernaderos tecnificados.",
                Category = "classic",
                EnginePower = "95 HP",
                Highlights = new[] { "Chasis ultracompacto", "Radio de giro cero", "Bajo consumo de combustible" }
            },
            new
            {
                Id = "CL-13",
                Name = "Rapid Racer",
                Brand = "TractorStore Classic",
                Price = 7500.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-13-BL.webp",
                Description = "La combinación perfecta entre velocidad de transporte terrestre y fuerza de toma de fuerza (PTO). Ideal para operaciones mixtas que requieren desplazamientos constantes.",
                Category = "classic",
                EnginePower = "180 HP",
                Highlights = new[] { "Transmisión Syncro-Fast", "Suspensión de cabina neumática", "Velocidad máxima optimizada" }
            },
            new
            {
                Id = "CL-15",
                Name = "Fieldmaster Classic",
                Brand = "TractorStore Classic",
                Price = 6200.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-15-PI.webp",
                Description = "El caballo de batalla tradicional de la marca. Mecánica 100% analógica de fácil mantenimiento, construida para resistir las condiciones climáticas más hostiles del entorno.",
                Category = "classic",
                EnginePower = "210 HP",
                Highlights = new[] { "Motor diésel de aspiración natural", "Cero componentes electrónicos críticos", "Tracción integral bloqueable" }
            },
            new
            {
                Id = "CL-01",
                Name = "Heritage Workhorse",
                Brand = "TractorStore Classic",
                Price = 5700.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-01-GR.webp",
                Description = "Maquinaria pesada inspirada en los diseños industriales clásicos pero con un tren motriz modernizado. Fiabilidad estructural garantizada para labranza profunda.",
                Category = "classic",
                EnginePower = "165 HP",
                Highlights = new[] { "Estructura de fundición nodular", "Bomba hidráulica de flujo constante", "Cabina panorámica clásica" }
            },
            new
            {
                Id = "AU-08",
                Name = "Field Pioneer",
                Brand = "TractorStore Autonomous",
                Price = 4500.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-08-WH.webp",
                Description = "Tractor robótico diseñado para reconocimiento inicial, preparación de camas de siembra y análisis de suelo en tiempo real mediante sensores integrados.",
                Category = "autonomous",
                EnginePower = "130 HP",
                Highlights = new[] { "Escaner de suelo por conductividad", "Navegación RTK de precisión centimétrica", "Chasis ligero antipinchazos" }
            },
            new
            {
                Id = "AU-02",
                Name = "SmartFarm Titan",
                Brand = "TractorStore Autonomous",
                Price = 4000.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-02-OG.webp",
                Description = "Unidad de alta potencia totalmente automatizada capaz de coordinar flotas de implementos dependientes mediante telemetría machine-to-machine.",
                Category = "autonomous",
                EnginePower = "380 HP",
                Highlights = new[] { "Procesamiento IA perimetral", "Doble antena satelital", "Sistema hidráulico inteligente" }
            },
            new
            {
                Id = "AU-07",
                Name = "Verde Voyager",
                Brand = "TractorStore Autonomous",
                Price = 4000.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-07-MT.webp",
                Description = "Especializado en operaciones de cultivos ecológicos y mantenimiento de cobertura vegetal sin intervención humana directa. Silencioso y eficiente.",
                Category = "autonomous",
                EnginePower = "120 HP",
                Highlights = new[] { "Transmisión eléctrica eco-drive", "Sensores multiespectrales", "Carga rápida inductiva" }
            },
            new
            {
                Id = "AU-05",
                Name = "EcoGrow Crop Commander",
                Brand = "TractorStore Autonomous",
                Price = 3400.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-05-ZH.webp",
                Description = "Diseñado específicamente para la pulverización selectiva y dosificación de precisión. Reduce drásticamente el uso de insumos químicos mediante análisis de imagen.",
                Category = "autonomous",
                EnginePower = "150 HP",
                Highlights = new[] { "Detección de malezas por visión artificial", "Tanque presurizado inteligente", "Control de secciones boquilla a boquilla" }
            },
            new
            {
                Id = "CL-12",
                Name = "Celerity Cruiser",
                Brand = "TractorStore Classic",
                Price = 3200.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-12-BL.webp",
                Description = "Tractor ágil ideal para tareas diarias de logística interna en la finca, transporte de forraje y remolques medianos.",
                Category = "classic",
                EnginePower = "110 HP",
                Highlights = new[] { "Transmisión Powershift ágil", "Cabina ergonómica certificada", "Frenos de disco en baño de aceite" }
            },
            new
            {
                Id = "CL-11",
                Name = "Scandinavia Sower",
                Brand = "TractorStore Classic",
                Price = 3100.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-11-SK.webp",
                Description = "Preparado con aislamiento térmico reforzado de fábrica y sistemas de precalentamiento de motor para trabajar en los inviernos y terrenos más exigentes.",
                Category = "classic",
                EnginePower = "140 HP",
                Highlights = new[] { "Paquete invernal ártico", "Calefacción de cabina auxiliar", "Alternador de alta capacidad" }
            },
            new
            {
                Id = "CL-09",
                Name = "TerraFirma Veneto",
                Brand = "TractorStore Classic",
                Price = 2950.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-09-BL.webp",
                Description = "Un clásico del diseño europeo, configurado con un centro de gravedad bajo especial para viñedos y cultivos en laderas o terrenos escarpados.",
                Category = "classic",
                EnginePower = "100 HP",
                Highlights = new[] { "Centro de gravedad bajo", "Estabilidad lateral mejorada", "Eje delantero oscilante" }
            },
            new
            {
                Id = "CL-07",
                Name = "Greenland Rover",
                Brand = "TractorStore Classic",
                Price = 2900.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-07-GR.webp",
                Description = "Enfocado en pasturas y ganadería. Su sistema hidráulico trasero tiene una excelente relación de levante para acoplar segadoras y rotoempacadoras.",
                Category = "classic",
                EnginePower = "125 HP",
                Highlights = new[] { "Toma de fuerza multidisco", "Control de profundidad mecánico", "Estructura antivuelco ROPS" }
            },
            new
            {
                Id = "CL-06",
                Name = "Danamark Steadfast",
                Brand = "TractorStore Classic",
                Price = 2800.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-06-MT.webp",
                Description = "Construcción sólida y duradera con enfoque en la eficiencia del operador a largo plazo. Un equipo confiable año tras año.",
                Category = "classic",
                EnginePower = "135 HP",
                Highlights = new[] { "Motor de alto torque a bajas RPM", "Dirección hidrostática", "Mantenimiento simplificado" }
            },
            new
            {
                Id = "CL-05",
                Name = "Countryside Commander",
                Brand = "TractorStore Classic",
                Price = 2700.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-05-PT.webp",
                Description = "Ideal para medianos productores que buscan la fuerza de un tractor grande sin perder la versatilidad de un chasis estándar.",
                Category = "classic",
                EnginePower = "150 HP",
                Highlights = new[] { "Excelente relación peso-potencia", "Inversor electrohidráulico", "Capacidad de levante trasera aumentada" }
            },
            new
            {
                Id = "CL-02",
                Name = "Falcon Crest Farm",
                Brand = "TractorStore Classic",
                Price = 2600.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-02-BL.webp",
                Description = "Diseñado para tareas agrícolas generales y operaciones ganaderas. Cuenta con un sistema hidráulico modular altamente configurable.",
                Category = "classic",
                EnginePower = "115 HP",
                Highlights = new[] { "Mandos finales planetarios", "Asiento con suspensión mecánica", "Salidas hidráulicas duales" }
            },
            new
            {
                Id = "CL-10",
                Name = "Global Gallant",
                Brand = "TractorStore Classic",
                Price = 2600.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-10-SD.webp",
                Description = "Configuración versátil estandarizada para mercados globales. Fácil operación y alta disponibilidad de componentes de recambio.",
                Category = "classic",
                EnginePower = "120 HP",
                Highlights = new[] { "Chasis de alta resistencia", "Filtro de aire de doble elemento", "Panel de instrumentos intuitivo" }
            },
            new
            {
                Id = "CL-03",
                Name = "Falcon Crest Work",
                Brand = "TractorStore Classic",
                Price = 2300.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-03-GR.webp",
                Description = "Variación enfocada al trabajo pesado en granja, cargadores frontales y movimiento de tierras ligero dentro del predio.",
                Category = "classic",
                EnginePower = "110 HP",
                Highlights = new[] { "Eje delantero reforzado", "Preinstalación para pala cargadora", "Embrague cerámico reforzado" }
            },
            new
            {
                Id = "CL-14",
                Name = "Caribbean Cruiser",
                Brand = "TractorStore Classic",
                Price = 2300.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-14-GR.webp",
                Description = "Diseño con protección anticorrosiva especial para zonas tropicales de alta humedad o plantaciones costeras de caña y arroz.",
                Category = "classic",
                EnginePower = "105 HP",
                Highlights = new[] { "Pintura con protección marina epóxica", "Sello de protección en rodamientos", "Radiador tropicalizado" }
            },
            new
            {
                Id = "CL-04",
                Name = "Broadfield Majestic",
                Brand = "TractorStore Classic",
                Price = 2200.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/CL-04-BL.webp",
                Description = "Tractor compacto y económico de bajo costo operativo, perfecto para pequeñas parcelas familiares o como soporte secundario.",
                Category = "classic",
                EnginePower = "85 HP",
                Highlights = new[] { "Consumo mínimo de diésel", "Fácil acceso a puntos de servicio", "Dimensiones reducidas" }
            },
            new
            {
                Id = "AU-06",
                Name = "FarmFleet Sovereign",
                Brand = "TractorStore Autonomous",
                Price = 2100.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-06-CZ.webp",
                Description = "Módulo autónomo especializado en patrullaje, mapeo de salud de cultivos mediante cámaras RGB e infrarrojas e índices NDVI.",
                Category = "autonomous",
                EnginePower = "90 HP",
                Highlights = new[] { "Cámara multiespectral integrada", "Transmisión de datos 5G/Radio", "Batería de estado sólido" }
            },
            new
            {
                Id = "AU-03",
                Name = "FutureHarvest Navigator",
                Brand = "TractorStore Autonomous",
                Price = 1600.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-03-TQ.webp",
                Description = "Pequeño rover autónomo multitarea, excelente para pasar entre hileras estrechas ejecutando desmalezado mecánico localizado.",
                Category = "autonomous",
                EnginePower = "75 HP",
                Highlights = new[] { "Navegación por visión estereoscópica", "Herramientas de desmalezado intercambiables", "Bajo impacto de pisada" }
            },
            new
            {
                Id = "AU-01",
                Name = "TerraFirma AutoCultivator T-300",
                Brand = "TractorStore Autonomous",
                Price = 1000.00m,
                Image = "https://blueprint.the-tractor.store/cdn/img/product/200/AU-01-SI.webp",
                Description = "Micro-unidad autónoma de entrada orientada a la automatización de tareas repetitivas a escala micro-agrícola o parcelas de prueba.",
                Category = "autonomous",
                EnginePower = "50 HP",
                Highlights = new[] { "Motorización eléctrica síncrona", "Programación abierta por bloques", "Arquitectura ligera modular" }
            }
        );

        // 3. Variantes de Producto Relacionadas (SKUs para pruebas de Stock y Carrito)
        modelBuilder.Entity<ProductVariant>().HasData(
            new { Sku = "AU04-STD", ProductId = "AU-04", name = "Standard Sapphire Core", Stock = 3, product_id = "AU-04" },
            new { Sku = "CL08-STD", ProductId = "CL-08", name = "Standard Wheels", Stock = 8, product_id = "CL-08" },
            new { Sku = "CL13-FAST", ProductId = "CL-13", name = "High Speed Axle Kit", Stock = 2, product_id = "CL-13" },
            new { Sku = "CL15-HD", ProductId = "CL-15", name = "Heavy Duty Iron Pack", Stock = 5, product_id = "CL-15" },
            new { Sku = "AU02-XL", ProductId = "AU-02", name = "Dual Antenna Pro Pack", Stock = 0, product_id = "AU-02" } // Sin stock intencional
        );
    }
}