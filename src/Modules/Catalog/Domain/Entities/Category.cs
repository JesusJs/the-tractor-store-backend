using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class Category
    {
        private Category() { } // EF Core

        public Category(string name, string slug)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre de la categoría es requerido.", nameof(name));
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("El slug de la categoría es requerido.", nameof(slug));

            Id = Guid.NewGuid();
            Name = name;
            Slug = slug;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Slug { get; private set; } = null!; // usado en /api/catalog/categories/{filter}
    }
}
