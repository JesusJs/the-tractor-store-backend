using Microsoft.EntityFrameworkCore;
using TractorEcommerce.Modules.Cart.Domain.Entities;


namespace TractorEcommerce.Modules.Cart.Infrastructure.Data
{
    public class CartDbContext : DbContext
    {
        public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
        {
        }

        // Ruta explícita completa para el DbSet
        public DbSet<CartEntity> Carts => Set<CartEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
        }
    }
}
