using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Api.Extensions;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Infrastructure.Events.Messaging;
using TractorEcommerce.Modules.Catalog.Infrastructure.Messaging;
using TractorEcommerce.Modules.Catalog.Infrastructure.Persistence;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Infrastructure.Persistence;
using TractorEcommerce.Modules.Sales.Infrastructure.Repository;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE PERSISTENCIA (POSTGRES)
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

// Registrar DbContext del Módulo de Catálogo
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Catalog.Infrastructure")));

// Registrar DbContext del Módulo de Ventas
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("TractorEcommerce.Modules.Sales.Infrastructure")));


// ==========================================
// 2. CONFIGURACIÓN DE KAFKA (MESSAGING)
// ==========================================
// Productor (Emisor de eventos)
builder.Services.AddSingleton<IEventBus, KafkaEventBus>();

// Consumidor (Receptor en segundo plano)
builder.Services.AddHostedService<KafkaOrderConsumer>();


// ==========================================
// 3. SEGURIDAD (JWT) Y CORS
// ==========================================
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("MfeCorsPolicy", policy =>
    {
        policy.AllowAnyOrigin() // En producción cambiar por dominios específicos
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


// ==========================================
// 4. ARQUITECTURA HEXAGONAL / INYECCIÓN DE DEPENDENCIAS
// ==========================================
builder.Services.AddScoped<ICatalogRepository, TractorEcommerce.Modules.Catalog.Infrastructure.Repository.SqlCatalogRepository>();
builder.Services.AddScoped<ISalesRepository, SqlSalesRepository>();
builder.Services.AddScoped<IInventoryService, SqlInventoryService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();

// Catalog Use Cases
builder.Services.AddScoped<GetHomeTeasersUseCase>();
builder.Services.AddScoped<GetCatalogCategoryUseCase>();
builder.Services.AddScoped<GetProductDetailUseCase>();
builder.Services.AddScoped<GetRecommendationsUseCase>();
builder.Services.AddScoped<GetStoresUseCase>();
builder.Services.AddScoped<GetInventoryStatusUseCase>();

// Sales Use Cases
builder.Services.AddScoped<CheckoutUseCase>();
builder.Services.AddScoped<AddToCartUseCase>();
builder.Services.AddScoped<RemoveFromCartUseCase>();
builder.Services.AddScoped<GetCartUseCase>();
builder.Services.AddScoped<GetMiniCartUseCase>();
builder.Services.AddScoped<GetOrderByIdUseCase>();


// ==========================================
// 5. SERVICIOS DEL SISTEMA / OPENAPI
// ==========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// ==========================================
// 6. AUTO-MIGRACIÓN DE BASE DE DATOS AL INICIAR
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var catalogContext = services.GetRequiredService<CatalogDbContext>();
        await catalogContext.Database.MigrateAsync();

        var salesContext = services.GetRequiredService<SalesDbContext>();
        await salesContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de base de datos.");
    }
}

// ==========================================
// PIPELINE DE PETICIONES HTTP (MIDDLEWARES)
// ==========================================
app.UseMiddleware<GlobalExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

// El orden de estos 3 middlewares es de vida o muerte:
app.UseCors("MfeCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();