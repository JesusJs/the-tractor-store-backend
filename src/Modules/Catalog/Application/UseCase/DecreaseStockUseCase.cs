using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Catalog.Application.Ports;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class DecreaseStockUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public DecreaseStockUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task ExecuteAsync(string sku, int quantity)
        {
            var variant = await _catalogRepository.GetVariantBySkuAsync(sku);
            if (variant != null)
            {
                // Descontar la cantidad comprada del stock actual
                variant.UpdateStock(variant.Stock - quantity);

                // Guardar los cambios en el CatalogDbContext a través del repositorio
                await _catalogRepository.UpdateVariantAsync(variant);
            }
        }
    }
}
