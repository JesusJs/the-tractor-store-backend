using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
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
            var products = await _catalogRepository.GetByCategoryAsync(filter);
            var productDtos = products.Select(p => new ProductItemDto(
                Id: p.Id,
                Name: p.Name,
                Brand: p.Brand,
                Price: p.Price,
                Image: p.Image,
                Variants: p.Variants.Select(v => v.Sku).ToList(),
                Description: p.Description,
                EnginePower: p.EnginePower,
                Stock: p.Variants.Sum(v => v.Stock)
            )).ToList();

            return new CatalogCategoryDto(
                Category: filter,
                Products: productDtos,
                AvailableFilters: new[] { "all", "classics", "autonomous" }
            );
        }
    }
}
