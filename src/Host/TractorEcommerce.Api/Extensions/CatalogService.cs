using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Catalog.Application.Ports;

namespace TractorEcommerce.Api.Extensions
{
    public class CatalogService : ICatalogService
    {
        private readonly ICatalogRepository _catalogRepository;

        public CatalogService(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<CatalogProductInfo?> GetProductBySkuAsync(string sku)
        {
            var variant = await _catalogRepository.GetVariantBySkuAsync(sku);
            if (variant == null)
                return null;

            var product = await _catalogRepository.GetByIdAsync(variant.ProductId);
            if (product == null)
                return null;

            return new CatalogProductInfo(
                ProductId: product.Id,
                Sku: variant.Sku,
                ProductName: product.Name,
                VariantName: variant.name,
                Price: product.Price,
                Image: product.Image
            );
        }
    }
}
