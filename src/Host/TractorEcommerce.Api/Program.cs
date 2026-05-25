using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Api.Extensions;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Infrastructure.Events.Messaging;
using TractorEcommerce.Modules.Catalog.Infrastructure.Messaging;
using TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;
using TractorEcommerce.Modules.Inventory.Infrastructure.Messaging;
using TractorEcommerce.Modules.Order.Infrastructure.Messaging;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE PERSISTENCIA (POSTGRES)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// Registrar DbContext del Módulo de Catálogo
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Catalog.Infrastructure")));
;

// NUEVO: Registrar DbContext del Módulo de Órdenes (Siguiendo tu patrón)
builder.Services.AddDbContext<TractorEcommerce.Modules.Order.Infrastructure.Data.OrderDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Order.Infrastructure")));

builder.Services.AddDbContext<TractorEcommerce.Modules.Inventory.Infrastructure.Data.InventoryDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Inventory.Infrastructure")));

builder.Services.AddDbContext<TractorEcommerce.Modules.Cart.Infrastructure.Data.CartDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Cart.Infrastructure")));

// ==========================================
// 2. CONFIGURACIÓN DE KAFKA (MESSAGING)
// ==========================================
// Productor (Emisor de eventos)
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();
builder.Services.AddHostedService<CatalogProductSyncedConsumer>();
builder.Services.AddHostedService<OrderPlacedConsumer>();
builder.Services.AddHostedService<InventoryResponseConsumer>();
// Consumidor (Receptor en segundo plano)
builder.Services.AddHostedService<KafkaOrderConsumer>();


// Levanta el hilo de escucha de Kafka para el Carrito en segundo plano
builder.Services.AddHostedService<TractorEcommerce.Modules.Cart.Infrastructure.Messaging.OrderPlacedConsumer>();
// ==========================================
// 3. SEGURIDAD (JWT) Y CORS
// ==========================================
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("MfeCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",  // Shell host
                "http://localhost:4201",  // mfe-explore
                "http://localhost:4202",  // mfe-decide
                "http://localhost:4203"   // mfe-checkout
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();        // Required for tractor_session cookie
    });
});


// ==========================================
// 4. ARQUITECTURA HEXAGONAL / INYECCIÓN DE DEPENDENCIAS
// ==========================================
builder.Services.AddScoped<ICatalogRepository, TractorEcommerce.Modules.Catalog.Infrastructure.Repository.SqlCatalogRepository>();
// Interfaces y Repositorios de Order
builder.Services.AddScoped<TractorEcommerce.Modules.Order.Application.Interfaces.Repository.IOrderRepository, TractorEcommerce.Modules.Order.Infrastructure.Repository.OrderRepository>();

// Casos de Uso de Order (Si los necesitas directo en controladores o handlers)
builder.Services.AddScoped<TractorEcommerce.Modules.Order.Application.UseCase.CheckoutUseCase>();
builder.Services.AddScoped<TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository.IInventoryRepository, TractorEcommerce.Modules.Inventory.Infrastructure.Repository.InventoryRepository>();

// Registrar Casos de Uso de Inventario
builder.Services.AddScoped<TractorEcommerce.Modules.Inventory.Application.UseCase.DeductStockOnOrderPlacedUseCase>();
//cart
builder.Services.AddScoped<TractorEcommerce.Modules.Cart.Application.Interfaces.Repository.ICartRepository, TractorEcommerce.Modules.Cart.Infrastructure.Repository.CartRepository>();

// Catalog Use Cases
builder.Services.AddScoped<GetHomeTeasersUseCase>();
builder.Services.AddScoped<GetCatalogCategoryUseCase>();
builder.Services.AddScoped<GetProductDetailUseCase>();
builder.Services.AddScoped<GetRecommendationsUseCase>();
builder.Services.AddScoped<GetStoresUseCase>();
builder.Services.AddScoped<GetInventoryStatusUseCase>();

// Sales Use Cases
builder.Services.AddScoped<TractorEcommerce.Modules.Cart.Application.UseCase.AddToCartUseCase>();
builder.Services.AddScoped<TractorEcommerce.Modules.Cart.Application.UseCase.GetCartUseCase>();
builder.Services.AddScoped<TractorEcommerce.Modules.Cart.Application.UseCase.RemoveFromCartUseCase>();


// ==========================================
// 5. SERVICIOS DEL SISTEMA / OPENAPI
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
// En tu Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Especifica tu origen exacto
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Esto es lo que permite enviar tus cookies/JWT
    });
});
var app = builder.Build();
app.UseCors("AllowMyFrontend");
// ==========================================
// 6. AUTO-MIGRACIÓN DE BASE DE DATOS AL INICIAR
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Migrar CatalogDbContext
    try
    {
        var context = services.GetRequiredService<CatalogDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones de CatalogDbContext aplicadas con éxito.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de CatalogDbContext.");
    }

    // Migrar OrderDbContext
    try
    {
        var context = services.GetRequiredService<TractorEcommerce.Modules.Order.Infrastructure.Data.OrderDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones de OrderDbContext aplicadas con éxito.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de OrderDbContext.");
    }

    // Migrar InventoryDbContext
    try
    {
        var context = services.GetRequiredService<TractorEcommerce.Modules.Inventory.Infrastructure.Data.InventoryDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones de InventoryDbContext aplicadas con éxito.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de InventoryDbContext.");
    }

    // Migrar CartDbContext
    try
    {
        var context = services.GetRequiredService<TractorEcommerce.Modules.Cart.Infrastructure.Data.CartDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Migraciones de CartDbContext aplicadas con éxito.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de CartDbContext.");
    }
}

// ==========================================
// PIPELINE DE PETICIONES HTTP (MIDDLEWARES)
// ==========================================
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Tractor Store API v1");
        options.RoutePrefix = "swagger"; // La URL será: http://localhost:YOUR_PORT/swagger
    });
}

//app.UseHttpsRedirection();

// El orden de estos 3 middlewares es de vida o muerte:
app.UseCors("MfeCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();