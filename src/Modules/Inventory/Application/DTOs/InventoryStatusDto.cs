using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Inventory.Application.DTOs
{
    public record InventoryStatusDto(string Sku, int Quantity);
}
