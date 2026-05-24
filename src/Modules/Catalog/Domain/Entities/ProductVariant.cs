using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class ProductVariant
    {
        public string Sku { get; private set; }
        public string ProductId { get; private set; }
        public string name { get; private set; }
        public int Stock { get; private set; }

        private ProductVariant() { }

        public ProductVariant(string sku, string productId, int stock)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("El SKU no puede estar vacío.");
            if (stock < 0) throw new ArgumentException("El stock no puede ser negativo.");

            Sku = sku;
            ProductId = productId;
            Stock = stock;
        }

        public void UpdateStock(int newStock)
        {
            if (newStock < 0) throw new ArgumentException("El stock no puede ser negativo.");
            Stock = newStock;
        }
    }
}
