using System.Collections.Generic;
using System.Threading.Tasks;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetHomeTeasersUseCase
    {
        public Task<IEnumerable<TeaserDto>> ExecuteAsync()
        {
            var teasers = new List<TeaserDto>
            {
                new TeaserDto(
                    Id: "AU-04",
                    Title: "Sapphire Sunworker 460R",
                    Image: "https://blueprint.the-tractor.store/cdn/img/product/200/AU-04-RD.webp",
                    Filter: "TractorStore Autonomous"
                ),
                new TeaserDto(
                    Id: "CL-08",
                    Title: "Holland Hamster",
                    Image: "https://blueprint.the-tractor.store/cdn/img/product/200/CL-08-GR.webp",
                    Filter: "TractorStore Classic"
                ),
            };
            return Task.FromResult<IEnumerable<TeaserDto>>(teasers);
        }
    }
}
