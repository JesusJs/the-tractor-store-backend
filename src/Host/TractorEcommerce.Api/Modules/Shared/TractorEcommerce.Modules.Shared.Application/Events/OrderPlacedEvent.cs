namespace TractorEcommerce.Api.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events
{
    public record OrderPlacedEvent(
    string OrderId,
    IEnumerable<OrderEventItem> Items,
    DateTime OccurredAt
);

    public record OrderEventItem(string Sku, int Quantity);
}
