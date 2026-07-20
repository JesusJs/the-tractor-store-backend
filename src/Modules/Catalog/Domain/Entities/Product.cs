using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class Product
    {
        private readonly List<ProductVariant> _variants = new();

        private Product() { } // EF Core

        public Product(
            string code,
            Guid categoryId,
            string name,
            string brand,
            string description,
            string imageUrl,
            string enginePower)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre es requerido.", nameof(name));

            Id = Guid.NewGuid();
            Code = code;
            CategoryId = categoryId;
            Name = name;
            Brand = brand;
            Description = description;
            ImageUrl = imageUrl;
            EnginePower = enginePower;
        }

        public Guid Id { get; private set; }
        public string Code { get; private set; } = null!; // referencia legible tipo "AU-04", no es PK
        public Guid CategoryId { get; private set; }
        public string Name { get; private set; } = null!;
        public string Brand { get; private set; } = null!;
        public string Description { get; private set; } = null!;
        public string ImageUrl { get; private set; } = null!;
        public string EnginePower { get; private set; } = null!;

        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    }

}