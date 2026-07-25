using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Application.DTOs
{
    public class CatalogDtos
    {
        // Endpoint 1: GET /api/catalog/home
        public record TeaserDto(
            string Id,
            string Title,
            string Image,
            string Filter
        );

        // Endpoint 2: GET /api/catalog/categories/{filter}
        public record CatalogCategoryDto(
            string Category,
            IEnumerable<ProductItemDto> Products,
            IEnumerable<string> AvailableFilters
        );

        public record ProductItemDto(
            Guid Id,
            Guid CategoryId,
            string Name,
            string Code,
            string Brand,
            string Image,
            string Description,
            string? EnginePower
        );

        // Endpoint 3: GET /api/catalog/products/{id}
        public record ProductDetailDto(
            Guid Id,
            string Name,
            string Brand,
            //decimal Price,
            string Description,
            string ImageUrl,
            string EnginePower
        );

        // Endpoint 6: GET /api/inventory/{sku}
        public record InventoryStatusDto(
            string Sku,
            int Stock
        );
    }
}
