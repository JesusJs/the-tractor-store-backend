using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.Helpers
{
    public class GetCatalogQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetCatalogQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<CatalogCategoryDto> ExecuteAsync(string filter)
        {
            // Regla: si es "all", traemos todo, si no, filtramos por la categoría en minúsculas
            var products = await _catalogRepository.GetByCategoryAsync(filter);

            var productDtos = products.Select(p => new ProductItemDto(
                Id: p.Id,
                Name: p.Name,
                Brand: p.Brand,
                Price: p.Price,
                Image: p.Image,
                Variants: p.Variants.Select(v => v.Sku).ToList(), // Extrae el array de strings de SKUs
                Description: p.Description,
                EnginePower: p.EnginePower,
                Stock: p.Variants.Sum(v => v.Stock)
            )).ToList();

            return new CatalogCategoryDto(
                Category: filter,
                Products: productDtos,
                AvailableFilters: new[] { "all", "classics", "autonomous" } // Filtros estipulados
            );
        }
    }
}
