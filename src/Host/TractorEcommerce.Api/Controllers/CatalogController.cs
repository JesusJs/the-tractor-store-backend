using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    [Authorize]
    public class CatalogController: ControllerBase
    {
        private static readonly List<ProductItemDto> MockProducts = new()
        {
            new ProductItemDto(
                "tx-001", "Autonomous Titan", "TractorCorp", 85000,
                "https://placehold.co/600x400/png?text=Autonomous+Titan",
                new[] { "TX-001-GPS", "TX-001-AI" },
                "Premium autonomous driving tractor.", "240 HP", 9
            ),
            new ProductItemDto(
                "tx-002", "Classic Vintage 1950", "HeritageIron", 45000,
                "https://placehold.co/600x400/png?text=Classic+Vintage",
                new[] { "TX-CLS-01" },
                "Beautifully restored post-war utility tractor.", "45 HP", 2
            )
         };

        [HttpGet("home")]
        public ActionResult GetHome()
        {
            var response = new { message = "Hola Mundo" };
            return Ok(response);
        }

        [HttpGet("categories/{filter}")]
        public ActionResult<CatalogCategoryDto> GetCategory(string filter)
        {
            var filteredProducts = filter.ToLower() == "all"
                ? MockProducts
                : MockProducts.Where(p => p.Description.ToLower().Contains(filter.ToLower()) || filter.ToLower() == "autonomous" && p.Id == "tx-001" || filter.ToLower() == "classics" && p.Id == "tx-002");

            var response = new CatalogCategoryDto(
                Category: filter,
                Products: filteredProducts,
                AvailableFilters: new[] { "all", "classics", "autonomous" }
            );

            return Ok(response);
        }

        [HttpGet("products/{id}")]
        public ActionResult<ProductDetailDto> GetProduct(string id)
        {
            var prod = MockProducts.FirstOrDefault(p => p.Id == id);
            if (prod == null) return NotFound(new { message = $"Producto {id} no encontrado." });

            var detail = new ProductDetailDto(
                Id: prod.Id,
                Name: prod.Name,
                Brand: prod.Brand,
                Price: prod.Price,
                Image: prod.Image,
                Description: prod.Description,
                Variants: prod.Variants,
                Highlights: new[]
                {
            "GPS Guided Autonomous System",
            $"{prod.EnginePower ?? "Standard"} High-Performance Power",
            "Dynamic Torque Control & Field Optimization",
            "Full Warranty & physical maintenance support included"
                }
            );

            return Ok(detail);
        }

        [HttpGet("stores")]
        public ActionResult<IEnumerable<object>> GetStores()
        {
            var stores = new[]
            {
        new { id = "store-central", name = "Central Headquarters", address = "Av. de la Maquinaria 404", city = "Madrid", image = "https://placehold.co/300x200" },
        new { id = "store-north", name = "North Hub", address = "Industrial Route 66, Km 12", city = "Burgos", image = "https://placehold.co/300x200" }
    };
            return Ok(stores);
        }
    }
}
