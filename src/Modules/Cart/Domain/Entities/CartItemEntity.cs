using System;
using System.Collections.Generic;
using System.Text;

    namespace TractorEcommerce.Modules.Cart.Domain.Entities
    {
        public class CartItemEntity
        {
            private CartItemEntity() { } // EF Core

            public CartItemEntity(Guid cartId, string sku, int quantity, decimal price)
            {
                if (string.IsNullOrWhiteSpace(sku))
                    throw new ArgumentException("El SKU es requerido.", nameof(sku));
                if (quantity <= 0)
                    throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(quantity));

                Id = Guid.NewGuid();
                CartId = cartId;
                Sku = sku;
                Quantity = quantity;
                Price = price;
            }

            public Guid Id { get; private set; }
            public Guid CartId { get; private set; }
            public string Sku { get; private set; } = null!;
            public int Quantity { get; private set; }
            public decimal Price { get; private set; }

            public decimal Total => Quantity * Price;

            public void IncreaseQuantity(int amount)
            {
                if (amount <= 0)
                    throw new ArgumentException("La cantidad a incrementar debe ser mayor a cero.", nameof(amount));
                Quantity += amount;
            }
        }
}
