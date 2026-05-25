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
        public DbSet<TractorEcommerce.Modules.Cart.Domain.Entities.Cart> Carts => Set<TractorEcommerce.Modules.Cart.Domain.Entities.Cart>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
        }
    }
}
