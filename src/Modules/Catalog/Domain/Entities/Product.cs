using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class Product
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Brand { get; private set; }
        public decimal Price { get; private set; }
        public string Image { get; private set; }
        public string Description { get; private set; }
        public string Category { get; private set; } // "classics", "autonomous", etc.
        public string? EnginePower { get; private set; }

        // DDD: Encapsulamos la lista de variantes para proteger la consistencia
        private readonly List<ProductVariant> _variants = new();
        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

        private readonly List<string> _highlights = new();
        public IReadOnlyCollection<string> Highlights => _highlights.AsReadOnly();

        // Constructor requerido por EF Core o mapeadores
        private Product() { }

        public Product(string id, string name, string brand, decimal price, string image, string description, string category, string? enginePower = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("El ID del producto no puede estar vacío.");
            if (price < 0) throw new ArgumentException("El precio no puede ser negativo.");

            Id = id;
            Name = name;
            Brand = brand;
            Price = price;
            Image = image;
            Description = description;
            Category = category.ToLower();
            EnginePower = enginePower;
        }

        public void AddVariant(string sku, int initialStock)
        {
            if (_variants.Any(v => v.Sku == sku))
                throw new InvalidOperationException($"La variante con SKU {sku} ya existe en este producto.");

            _variants.Add(new ProductVariant(sku, Id, initialStock));
        }

        public void AddHighlight(string highlight)
        {
            if (!string.IsNullOrWhiteSpace(highlight))
                _highlights.Add(highlight);
        }
    }

}