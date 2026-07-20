using System;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class Store
    {
        private Store() { } // EF Core

        public Store(string name, string address, string city, string imageUrl)
        {
            Id = Guid.NewGuid();
            Name = name;
            Address = address;
            City = city;
            ImageUrl = imageUrl;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Address { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public string ImageUrl { get; private set; } = null!;
    }
}
