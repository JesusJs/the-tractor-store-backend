using System;

namespace TractorEcommerce.Modules.Catalog.Domain.Entities
{
    public class Store
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string Image { get; private set; }

        private Store() { }

        public Store(string id, string name, string address, string city, string image)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Store ID cannot be empty.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Store Name cannot be empty.");

            Id = id;
            Name = name;
            Address = address;
            City = city;
            Image = image;
        }
    }
}
