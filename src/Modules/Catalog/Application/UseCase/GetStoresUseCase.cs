using System.Linq;
using TractorEcommerce.Modules.Catalog.Application.DTOs;
using TractorEcommerce.Modules.Catalog.Application.Ports;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetStoresUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetStoresUseCase(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<StoreResponse>> ExecuteAsync()
        {
            var stores = await _catalogRepository.GetStoresAsync();
            return stores.Select(s => new StoreResponse(
             s.Id,
             s.Name,
             s.Address ?? string.Empty,
             s.City ?? string.Empty,
             s.Image ?? string.Empty
            )).ToList();
        }
    }
}
