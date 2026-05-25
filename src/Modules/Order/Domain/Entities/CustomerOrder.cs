using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Order.Domain.Entities
{
    public class CustomerOrder
    {
        public Guid Id { get; private set; }
        public string CustomerId { get; private set; }
        private readonly List<OrderLineItem> _items = new();
        public IReadOnlyCollection<OrderLineItem> Items => _items.AsReadOnly();
        public decimal TotalAmount => _items.Sum(item => item.Total);
        public DateTime CreatedAt { get; private set; }
        public string Status { get; private set; } // Ej: "Pending", "Completed", "Cancelled"

        private CustomerOrder()
        {
            CustomerId = null!;
        }

        public CustomerOrder(Guid id, string customerId, List<OrderLineItem> items)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException("El CustomerId no puede estar vacío.");

            if (items == null || !items.Any())
                throw new ArgumentException("Una orden debe contener al menos un ítem.");

            Id = id;
            CustomerId = customerId;
            _items = items;
            CreatedAt = DateTime.UtcNow;
            Status = "Pending";
        }

        public void Complete()
        {
            Status = "Completed";
        }
    }
}
