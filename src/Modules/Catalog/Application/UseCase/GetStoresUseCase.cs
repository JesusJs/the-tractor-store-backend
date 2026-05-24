using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<object>> ExecuteAsync()
        {
            var stores = await _catalogRepository.GetStoresAsync();
            return stores.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                address = s.Address,
                city = s.City,
                image = s.Image
            }).ToList();
        }
    }
}
