using System;
using System.Collections.Generic;

namespace TractorEcommerce.Modules.Sales.Domain.Entities
{
    public class OrderReceipt
    {
        public string Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string StoreId { get; private set; }
        public string? ExtraPickups { get; private set; }
        public decimal SubTotal { get; private set; }
        public decimal Tax { get; private set; }
        public decimal Total { get; private set; }
        public DateTime PlacedAt { get; private set; }

        private readonly List<OrderReceiptItem> _items = new();
        public IReadOnlyCollection<OrderReceiptItem> Items => _items.AsReadOnly();

        private OrderReceipt() { }

        public OrderReceipt(string id, string firstName, string lastName, string storeId, string? extraPickups, decimal subTotal, decimal tax, decimal total, DateTime placedAt)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            StoreId = storeId;
            ExtraPickups = extraPickups;
            SubTotal = subTotal;
            Tax = tax;
            Total = total;
            PlacedAt = placedAt;
        }

        public void AddItem(OrderReceiptItem item)
        {
            _items.Add(item);
        }
    }
}
