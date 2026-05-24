using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Inventory.Domain.Entities
{
    public class SkuStock
    {
        public string Sku { get; private set; }
        public int AvailableStock { get; private set; }

        public SkuStock(string sku, int stock)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU no puede estar vacío.");

            if (stock < 0)
                throw new ArgumentException("El stock inicial no puede ser negativo.");

            Sku = sku.ToUpper();
            AvailableStock = stock;
        }

        // Regla de negocio crítica: No podemos vender lo que no tenemos
        public void DeductStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad a descontar debe ser mayor a cero.");

            if (AvailableStock < quantity)
                throw new InvalidOperationException($"Stock insuficiente para el SKU: {Sku}. Disponible: {AvailableStock}, Solicitado: {quantity}");

            AvailableStock -= quantity;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("La cantidad a añadir debe ser mayor a cero.");

            AvailableStock += quantity;
        }
    }
}
