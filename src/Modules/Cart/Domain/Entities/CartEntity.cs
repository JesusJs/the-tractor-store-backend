using System;
using System.Collections.Generic;
using System.Linq;

namespace TractorEcommerce.Modules.Cart.Domain.Entities
{
    public class CartEntity
    {
        private readonly List<CartItemEntity> _items = new();

        // Constructor requerido por EF Core para la reconstrucción desde la BD
        private CartEntity() { }

        public CartEntity(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("El UserId/SessionId es requerido para crear un carrito.", nameof(userId));

            Id = Guid.NewGuid();
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }
        public string UserId { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; }

        public IReadOnlyCollection<CartItemEntity> Items => _items.AsReadOnly();

        public int TotalItems => _items.Sum(i => i.Quantity);
        public decimal SubTotal => _items.Sum(i => i.Total);
        public decimal Tax => SubTotal * 0.19m;
        public decimal Total => SubTotal + Tax;

        public void AddItem(string sku, int quantity, decimal price)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(quantity));

            var existingItem = _items.FirstOrDefault(i => i.Sku == sku);
            if (existingItem != null)
            {
                existingItem.IncreaseQuantity(quantity); // muta, no reemplaza
            }
            else
            {
                _items.Add(new CartItemEntity(Id, sku, quantity, price));
            }
        }

        public void RemoveItem(string sku)
        {
            var item = _items.FirstOrDefault(i => i.Sku == sku);
            if (item != null)
                _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}