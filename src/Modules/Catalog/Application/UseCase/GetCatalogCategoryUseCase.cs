using OpenQA.Selenium;
using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetCatalogCategoryUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetCatalogCategoryUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<CatalogCategoryDto> ExecuteAsync(string filter)
        {
            List<Product> products;
            string categoryLabel;

            if (string.Equals(filter, "all", StringComparison.OrdinalIgnoreCase))
            {
                products = await _catalogRepository.GetAllProductsAsync();
                categoryLabel = "all";
            }
            else
            {
                var category = await _catalogRepository.GetCategoryBySlug(filter);

                if (category is null)
                    throw new NotFoundException($"La categoría '{filter}' no existe."); // o el manejo de errores que uses

                products = await _catalogRepository.GetProductsByIdAsync(category.Id);
                categoryLabel = category.Slug;
            }

            var productDtos = products.Select(p => new ProductItemDto(
                Id: p.Id,
                CategoryId: p.CategoryId,
                Code: p.Code,
                Name: p.Name,
                Image: p.ImageUrl,
                Brand: p.Brand,
                Description: p.Description,
                EnginePower: p.EnginePower
            )).ToList();

            return new CatalogCategoryDto(
                Category: categoryLabel,
                Products: productDtos,
                AvailableFilters: new[] { "all", "classic", "autonomous" } // corregido: coincide con los slugs reales
            );
        }
    }
}
