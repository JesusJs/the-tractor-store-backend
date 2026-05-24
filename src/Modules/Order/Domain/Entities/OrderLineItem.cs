using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Order.Domain.Entities
{
    public record OrderLineItem(Guid Id, string Sku, int Quantity, decimal UnitPrice)
    {
        public string ProductName;

        public decimal Total => UnitPrice * Quantity;
    }
}
