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
            string Id,
            string Name,
            string Brand,
            decimal Price,
            string Image,
            IEnumerable<string> Variants,
            string Description,
            string? EnginePower,
            int Stock
        );

        // Endpoint 3: GET /api/catalog/products/{id}
        public record ProductDetailDto(
            string Id,
            string Name,
            string Brand,
            decimal Price,
            string Image,
            string Description,
            IEnumerable<string> Variants,
            IEnumerable<string> Highlights
        );

        // Endpoint 6: GET /api/inventory/{sku}
        public record InventoryStatusDto(
            string Sku,
            int Stock
        );
    }
}
