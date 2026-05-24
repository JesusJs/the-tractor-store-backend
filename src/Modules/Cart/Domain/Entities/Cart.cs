using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Domain.Entities
{

    public class Cart
    {
        private readonly List<CartItem> _items = new();

        // Constructor requerido por EF Core para la reconstrucción desde la BD
        private Cart() { }

        public Cart(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("El UserId/SessionId es requerido para crear un carrito.", nameof(userId));

            UserId = userId;
        }

        public string UserId { get; private set; } = null!;
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

        // Propiedades calculadas dinámicas o con setters privados para EF Core
        public int TotalItems => _items.Sum(i => i.Quantity);
        public decimal SubTotal => _items.Sum(i => i.Total);
        public decimal Tax => SubTotal * 0.19m; // Ejemplo IVA Colombia 19%
        public decimal Total => SubTotal + Tax;

        public void AddItem(string sku, int quantity, decimal price)
        {
            if (quantity <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(quantity));

            var existingItem = _items.FirstOrDefault(i => i.Sku == sku);

            if (existingItem != null)
            {
                // Como el record es inmutable, lo reemplazamos sumando la cantidad
                _items.Remove(existingItem);
                _items.Add(existingItem with { Quantity = existingItem.Quantity + quantity });
            }
            else
            {
                _items.Add(new CartItem(sku, quantity, price));
            }
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
