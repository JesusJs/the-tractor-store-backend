using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogRepository _catalogRepository;

        public CatalogController(ICatalogRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        [HttpGet("home")]
        public ActionResult<IEnumerable<TeaserDto>> GetHome()
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
            return Ok(teasers);
        }

        [HttpGet("categories/{filter}")]
        public async Task<ActionResult<CatalogCategoryDto>> GetCategory(string filter)
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

            var response = new CatalogCategoryDto(
                Category: filter,
                Products: productDtos,
                AvailableFilters: new[] { "all", "classics", "autonomous" }
            );

            return Ok(response);
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(string id)
        {
            var prod = await _catalogRepository.GetByIdAsync(id);
            if (prod == null) return NotFound(new { message = $"Producto {id} no encontrado." });

            var detail = new ProductDetailDto(
                Id: prod.Id,
                Name: prod.Name,
                Brand: prod.Brand,
                Price: prod.Price,
                Image: prod.Image,
                Description: prod.Description,
                Variants: prod.Variants.Select(v => v.Sku).ToList(),
                Highlights: prod.Highlights.ToList()
            );

            return Ok(detail);
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<IEnumerable<ProductItemDto>>> GetRecommendations([FromQuery] string? skus)
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

            var dtos = recommendedProducts.Select(p => new ProductItemDto(
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

            return Ok(dtos);
        }

        [HttpGet("stores")]
        public async Task<ActionResult<IEnumerable<object>>> GetStores()
        {
            var stores = await _catalogRepository.GetStoresAsync();
            var response = stores.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                address = s.Address,
                city = s.City,
                image = s.Image
            });
            return Ok(response);
        }
    }
}
