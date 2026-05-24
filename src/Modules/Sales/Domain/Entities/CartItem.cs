using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Domain.Entities
{
    public class CartItem
    {
        public string ProductId { get; private set; }
        public string VariantId { get; private set; } // Representa el SKU
        public string ProductName { get; private set; }
        public string VariantName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public string Image { get; private set; }

        private CartItem() { } // Requerido para ORM

        public CartItem(string productId, string variantId, string productName, string variantName, decimal price, string image, int quantity = 1)
        {
            ProductId = productId;
            VariantId = variantId;
            ProductName = productName;
            VariantName = variantName;
            Price = price;
            Image = image;
            Quantity = quantity;
        }

        public void IncrementQuantity()
        {
            Quantity += 1; // Requisito: Incrementar en +1 si ya existe
        }
    }
}
