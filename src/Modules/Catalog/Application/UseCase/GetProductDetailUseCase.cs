using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetProductDetailUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetProductDetailUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<ProductDetailDto?> ExecuteAsync(string id)
        {
            var prod = await _catalogRepository.GetByIdAsync(id);
            if (prod == null) return null;

            return new ProductDetailDto(
                Id: prod.Id,
                Name: prod.Name,
                Brand: prod.Brand,
                Price: prod.Price,
                Image: prod.Image,
                Description: prod.Description,
                Variants: prod.Variants.Select(v => v.Sku).ToList(),
                Highlights: prod.Highlights.ToList()
            );
        }
    }
}
