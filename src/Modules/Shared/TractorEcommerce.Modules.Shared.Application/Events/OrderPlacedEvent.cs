using TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events
{
    public record OrderPlacedEvent(
    Guid OrderId,
    string CustomerId,
    List<CartItemDto> Items,
    DateTime PlacedAt
    );
}
