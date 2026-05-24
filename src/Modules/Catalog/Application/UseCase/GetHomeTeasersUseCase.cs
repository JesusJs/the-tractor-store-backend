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
                    Id: "teaser-classics",
                    Title: "Classic Vintage Tractors",
                    Image: "https://placehold.co/600x400/png?text=Classic+Vintage",
                    Filter: "classics"
                ),
                new TeaserDto(
                    Id: "teaser-autonomous",
                    Title: "Autonomous & AI Tractors",
                    Image: "https://placehold.co/600x400/png?text=Autonomous+Titan",
                    Filter: "autonomous"
                )
            };
            return Task.FromResult<IEnumerable<TeaserDto>>(teasers);
        }
    }
}
