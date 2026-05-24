using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Application.UseCase
{
    public class GetRecommendationsUseCase
    {
        private readonly ICatalogRepository _catalogRepository;

        public GetRecommendationsUseCase(ICatalogRepository catalogRepository)
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
}
