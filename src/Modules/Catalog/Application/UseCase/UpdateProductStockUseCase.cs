using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Catalog.Application.Events;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class UpdateProductStockUseCase
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IEventBus _eventBus; // 💡 Inyectamos tu bus genérico

        public UpdateProductStockUseCase(ICatalogRepository catalogRepository, IEventBus eventBus)
        {
            _catalogRepository = catalogRepository;
            _eventBus = eventBus;
        }
        public async Task ExecuteAsync(string sku, int addedStock)
        {
            // 1. Buscamos la variante con el método real de tu interfaz
            var variant = await _catalogRepository.GetVariantBySkuAsync(sku);
            if (variant == null) return;

            // 2. Modificamos el stock en la entidad de Catálogo
            // (Si tu ProductVariant usa DDD con un método semántico como variant.AddStock(), úsalo aquí. 
            // Si es una propiedad con setter público/init, haz la suma directa, por ejemplo: variant.Stock += addedStock)
            variant.UpdateStock(variant.Stock + addedStock);

            // Persistimos usando el método exacto de tu ICatalogRepository 🛠️
            await _catalogRepository.UpdateVariantAsync(variant);

            // 3. Publicamos el evento usando tu KafkaEventBus genérico 🚀
            var integrationEvent = new ProductStockUpdatedEvent(variant.Sku, variant.Stock);

            await _eventBus.PublishAsync(
                topic: "catalog.products.stock-updated",
                key: variant.Sku,
                message: integrationEvent
            );
        }
    }
}
