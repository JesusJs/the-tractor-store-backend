using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Domain.Entities
{
    public class ShoppingCart
    {
        // El Id del carrito será el SessionId único almacenado en la cookie HttpOnly
        public string Id { get; private set; }
        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        public ShoppingCart(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El Id del carrito (SessionId) no puede estar vacío.");
            Id = id;
        }

        public void AddItem(CartItem newItem)
        {
            var existingItem = _items.FirstOrDefault(i => i.Sku.Equals(newItem.Sku, StringComparison.OrdinalIgnoreCase));

            if (existingItem != null)
            {
                // Como CartItem es un record inmutable, usamos la expresión 'with' para actualizar la cantidad
                _items.Remove(existingItem);
                _items.Add(existingItem with { Quantity = existingItem.Quantity + newItem.Quantity });
            }
            else
            {
                _items.Add(newItem);
            }
        }

        public void RemoveItem(string sku)
        {
            var item = _items.FirstOrDefault(i => i.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public void Clear() => _items.Clear();

        public decimal TotalAmount => _items.Sum(item => item.Total);
    }
}
