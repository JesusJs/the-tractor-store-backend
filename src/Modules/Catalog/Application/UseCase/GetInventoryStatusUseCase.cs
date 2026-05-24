using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetInventoryStatusUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetInventoryStatusUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<InventoryStatusDto?> ExecuteAsync(string sku)
        {
            var variant = await _catalogRepository.GetVariantBySkuAsync(sku);
            if (variant == null) return null;

            return new InventoryStatusDto(sku, variant.Stock);
        }
    }
}
