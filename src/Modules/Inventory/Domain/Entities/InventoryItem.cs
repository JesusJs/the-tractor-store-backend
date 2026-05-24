using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Inventory.Domain.Entities
{
    public class InventoryItem
    {
        public Guid Id { get; private set; }
        public string Sku { get; private set; }
        public int AvailableStock { get; private set; }

        // 💡 LA SOLUCIÓN: Propiedad computada (Read-only) para compatibilidad
        // Apunta directamente a AvailableStock, así que siempre están sincronizadas.
        public int Quantity => AvailableStock;

        public DateTime LastUpdatedAt { get; private set; }

        private InventoryItem() { }

        public InventoryItem(Guid id, string sku, int availableStock)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU no puede estar vacío.", nameof(sku));

            if (availableStock < 0)
                throw new ArgumentOutOfRangeException(nameof(availableStock), "El stock no puede ser negativo.");

            Id = id;
            Sku = sku.ToUpper().Trim();
            AvailableStock = availableStock;
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(newQuantity), "El stock no puede ser negativo.");

            AvailableStock = newQuantity;
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void DeductStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad a descontar debe ser mayor a cero.", nameof(quantity));

            if (AvailableStock - quantity < 0)
                throw new InvalidOperationException($"Stock insuficiente para el SKU {Sku}. Disponibles: {AvailableStock}, Solicitados: {quantity}");

            AvailableStock -= quantity;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}
