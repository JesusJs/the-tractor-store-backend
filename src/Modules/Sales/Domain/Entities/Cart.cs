using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Domain.Entities
{
    public class Cart
    {
        public string UserId { get; private set; } // Cookie, token o id anónimo

        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public int TotalItems => _items.Sum(i => i.Quantity);
        public decimal SubTotal => _items.Sum(i => i.Price * i.Quantity);

        // Requisito: Impuesto al 21% con redondeo matemático exacto
        public decimal Tax => Math.Round(SubTotal * 0.21m, 2);
        public decimal Total => SubTotal + Tax;

        private Cart() { }

        public Cart(string userId)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        }

        public void AddItem(string productId, string sku, string productName, string variantName, decimal price, string image)
        {
            var existingItem = _items.FirstOrDefault(i => i.VariantId == sku);
            if (existingItem != null)
            {
                existingItem.IncrementQuantity(); // Incrementa +1 en lugar de duplicar
            }
            else
            {
                _items.Add(new CartItem(productId, sku, productName, variantName, price, image));
            }
        }

        public void RemoveItem(string sku)
        {
            var item = _items.FirstOrDefault(i => i.VariantId == sku);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public void Clear()
        {
            _items.Clear(); // Requisito al consolidar la orden
        }
    }
}
