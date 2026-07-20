using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class ProductVariant
    {
        private ProductVariant() { } // EF Core

        public ProductVariant(
            Guid productId,
            string sku,
            string name,
            decimal price,
            IReadOnlyCollection<string> highlights)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU es requerido.", nameof(sku));

            Id = Guid.NewGuid();
            ProductId = productId;
            Sku = sku;
            Name = name;
            Price = price;
            Highlights = highlights?.ToArray() ?? Array.Empty<string>();
        }

        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public decimal Price { get; private set; }
        public string[] Highlights { get; private set; } = Array.Empty<string>();

        // Nota: "Stock" ya NO vive aquí. Vive en inventory_stocks, otro módulo.
    }
}
