using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Order.Application.DTOs
{
    public record OrderPayloadDto(
      string FirstName,
      string LastName,
      string StoreId,
      string? ExtraPickups,
      List<OrderPayloadItemDto> Items
  );

    public record OrderPayloadItemDto(
        string ProductId,
        string VariantId,
        string ProductName,
        string? VariantName,
        decimal Price,
        int Quantity,
        string? Image
    );

    public record OrderReceiptDto(
        string Id,
        string FirstName,
        string LastName,
        string StoreId,
        string? ExtraPickups,
        List<OrderItemDetailDto> Items,
        decimal SubTotal,
        decimal Tax,
        decimal Total,
        DateTime PlacedAt
    );

    public record OrderItemDetailDto(
        string ProductId,
        string VariantId,
        string ProductName,
        string? VariantName,
        decimal Price,
        int Quantity,
        string? Image
    );
}
