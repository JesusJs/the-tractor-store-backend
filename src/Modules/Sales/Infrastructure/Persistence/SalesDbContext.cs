using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Sales.Domain;
using TractorEcommerce.Modules.Sales.Domain.Entities; // Aquí irán tus entidades del dominio

namespace TractorEcommerce.Modules.Sales.Infrastructure.Persistence;

public class SalesDbContext : DbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<OrderReceipt> Orders => Set<OrderReceipt>();

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

        // Configuración de la Orden (OrderReceipt)
        modelBuilder.Entity<OrderReceipt>(builder =>
        {
            builder.ToTable("orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).HasMaxLength(50);
            builder.Property(o => o.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(o => o.LastName).HasMaxLength(100).IsRequired();
            builder.Property(o => o.StoreId).HasMaxLength(50).IsRequired();
            builder.Property(o => o.ExtraPickups).HasMaxLength(500);
            builder.Property(o => o.SubTotal).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.Tax).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.Total).HasPrecision(18, 2).IsRequired();
            builder.Property(o => o.PlacedAt).IsRequired();

            builder.Metadata.FindNavigation(nameof(OrderReceipt.Items))?
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsMany(o => o.Items, itemBuilder =>
            {
                itemBuilder.ToTable("order_items");
                itemBuilder.WithOwner().HasForeignKey("order_id");
                itemBuilder.HasKey("order_id", nameof(OrderReceiptItem.Sku));

                itemBuilder.Property(i => i.ProductId).HasMaxLength(50).IsRequired();
                itemBuilder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
                itemBuilder.Property(i => i.ProductName).HasMaxLength(150).IsRequired();
                itemBuilder.Property(i => i.VariantName).HasMaxLength(100).IsRequired();
                itemBuilder.Property(i => i.Price).HasPrecision(18, 2).IsRequired();
                itemBuilder.Property(i => i.Quantity).IsRequired();
                itemBuilder.Property(i => i.Image).HasMaxLength(500);
            });
        });
    }
}