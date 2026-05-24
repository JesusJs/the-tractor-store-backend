using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetHomeTeasersQueryHandler
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

    public class GetCatalogCategoryQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetCatalogCategoryQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<CatalogCategoryDto> ExecuteAsync(string filter)
        {
            var products = await _catalogRepository.GetByCategoryAsync(filter);
            var productDtos = products.Select(p => new ProductItemDto(
                Id: p.Id,
                Name: p.Name,
                Brand: p.Brand,
                Price: p.Price,
                Image: p.Image,
                Variants: p.Variants.Select(v => v.Sku).ToList(),
                Description: p.Description,
                EnginePower: p.EnginePower,
                Stock: p.Variants.Sum(v => v.Stock)
            )).ToList();

            return new CatalogCategoryDto(
                Category: filter,
                Products: productDtos,
                AvailableFilters: new[] { "all", "classics", "autonomous" }
            );
        }
    }

    public class GetProductDetailQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetProductDetailQueryHandler(ICatalogRepository catalogRepository)
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

    public class GetRecommendationsQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetRecommendationsQueryHandler(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<IEnumerable<ProductItemDto>> ExecuteAsync(string? skus)
        {
            IEnumerable<Product> recommendedProducts;
            if (string.IsNullOrWhiteSpace(skus))
            {
                recommendedProducts = await _catalogRepository.GetByCategoryAsync("all");
            }
            else
            {
                var skuList = skus.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var matchedProducts = await _catalogRepository.GetProductsBySkusAsync(skuList);

                if (matchedProducts.Any())
                {
                    var categories = matchedProducts.Select(p => p.Category).Distinct().ToList();
                    var allOfSameCategories = new List<Product>();
                    foreach (var category in categories)
                    {
                        var prods = await _catalogRepository.GetByCategoryAsync(category);
                        allOfSameCategories.AddRange(prods);
                    }
                    
                    recommendedProducts = allOfSameCategories
                        .Where(p => !matchedProducts.Any(mp => mp.Id == p.Id))
                        .DistinctBy(p => p.Id);
                }
                else
                {
                    recommendedProducts = await _catalogRepository.GetByCategoryAsync("all");
                }
            }

            return recommendedProducts.Select(p => new ProductItemDto(
                Id: p.Id,
                Name: p.Name,
                Brand: p.Brand,
                Price: p.Price,
                Image: p.Image,
                Variants: p.Variants.Select(v => v.Sku).ToList(),
                Description: p.Description,
                EnginePower: p.EnginePower,
                Stock: p.Variants.Sum(v => v.Stock)
            )).ToList();
        }
    }

    public class GetStoresQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetStoresQueryHandler(ICatalogRepository catalogRepository)
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

    public class GetInventoryStatusQueryHandler
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetInventoryStatusQueryHandler(ICatalogRepository catalogRepository)
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
