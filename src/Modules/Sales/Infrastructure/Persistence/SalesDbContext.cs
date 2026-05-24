using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Sales.Domain;
using TractorEcommerce.Modules.Sales.Domain.Entities; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");

        // Configuración del Agregado de Carrito
        modelBuilder.Entity<Cart>(builder =>
        {
            builder.ToTable("carts");
            builder.HasKey(c => c.UserId); // El ID de Sesión Anónima o Usuario es la PK

            builder.Property(c => c.UserId).HasMaxLength(200);

            // DDD: Forzamos a EF Core a mapear la propiedad de navegación a través del campo privado '_items'
            builder.Metadata.FindNavigation(nameof(Cart.Items))?
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // Relación de propiedad total (Owned Entity) hacia los items del carrito
            builder.OwnsMany(c => c.Items, itemBuilder =>
            {
                itemBuilder.ToTable("cart_items");
                itemBuilder.WithOwner().HasForeignKey("cart_user_id");

                // Llave primaria compuesta compuesta por el usuario y el SKU (VariantId)
                itemBuilder.HasKey("cart_user_id", nameof(CartItem.VariantId));

                itemBuilder.Property(i => i.ProductId).HasMaxLength(50).IsRequired();
                itemBuilder.Property(i => i.VariantId).HasMaxLength(50).IsRequired();
                itemBuilder.Property(i => i.ProductName).HasMaxLength(150).IsRequired();
                itemBuilder.Property(i => i.VariantName).HasMaxLength(100).IsRequired();
                itemBuilder.Property(i => i.Price).HasPrecision(18, 2).IsRequired();
                itemBuilder.Property(i => i.Quantity).IsRequired();
                itemBuilder.Property(i => i.Image).HasMaxLength(500);
            });
        });
    }
}