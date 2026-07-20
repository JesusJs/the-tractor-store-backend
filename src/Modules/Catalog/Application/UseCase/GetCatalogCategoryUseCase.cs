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

        //public async Task<CatalogCategoryDto> ExecuteAsync(string filter)
        //{
        //    var products = await _catalogRepository.GetByCategoryAsync(filter);
        //    var productDtos = products.Select(p => new ProductItemDto(
        //        Id: p.Id,
        //        categoryId: p.CategoryId,
        //        code: p.Code,
        //        Name: p.Name,
        //        Image: p.ImageUrl,
        //        brand: p.Brand,
        //        Description: p.Description,
        //        EnginePower: p.EnginePower
        //    )).ToList();

        //    return new CatalogCategoryDto(
        //        Category: filter,
        //        Products: productDtos,
        //        AvailableFilters: new[] { "all", "classics", "autonomous" }
        //    );
        //}
    }
}
