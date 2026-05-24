using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Shared.Application.Events
{
    public record CheckoutRequestedEvent(
    string CartId,
    string CustomerId,
    List<CartItemDto> Items,
    decimal TotalAmount
);
    public record CartItemDto(string Sku, int Quantity, decimal Price);
}
