using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Domain.Entities
{
    public record CartItem(string Sku, int Quantity, decimal Price)
    {
        // Lógica de dominio para calcular el total de esta línea
        public decimal Total => Price * Quantity;
    }
}
