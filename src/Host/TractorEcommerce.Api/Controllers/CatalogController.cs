using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly GetHomeTeasersQueryHandler _getHomeTeasersHandler;
        private readonly GetCatalogCategoryQueryHandler _getCatalogCategoryHandler;
        private readonly GetProductDetailQueryHandler _getProductDetailHandler;
        private readonly GetRecommendationsQueryHandler _getRecommendationsHandler;
        private readonly GetStoresQueryHandler _getStoresHandler;

        public CatalogController(
            GetHomeTeasersQueryHandler getHomeTeasersHandler,
            GetCatalogCategoryQueryHandler getCatalogCategoryHandler,
            GetProductDetailQueryHandler getProductDetailHandler,
            GetRecommendationsQueryHandler getRecommendationsHandler,
            GetStoresQueryHandler getStoresHandler)
        {
            _getHomeTeasersHandler = getHomeTeasersHandler;
            _getCatalogCategoryHandler = getCatalogCategoryHandler;
            _getProductDetailHandler = getProductDetailHandler;
            _getRecommendationsHandler = getRecommendationsHandler;
            _getStoresHandler = getStoresHandler;
        }

        [HttpGet("home")]
        public async Task<ActionResult<IEnumerable<TeaserDto>>> GetHome()
        {
            var teasers = await _getHomeTeasersHandler.ExecuteAsync();
            return Ok(teasers);
        }

        [HttpGet("categories/{filter}")]
        public async Task<ActionResult<CatalogCategoryDto>> GetCategory(string filter)
        {
            var categoryDto = await _getCatalogCategoryHandler.ExecuteAsync(filter);
            return Ok(categoryDto);
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(string id)
        {
            var detail = await _getProductDetailHandler.ExecuteAsync(id);
            if (detail == null)
            {
                return NotFound(new { message = $"Producto {id} no encontrado." });
            }
            return Ok(detail);
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<IEnumerable<ProductItemDto>>> GetRecommendations([FromQuery] string? skus)
        {
            var dtos = await _getRecommendationsHandler.ExecuteAsync(skus);
            return Ok(dtos);
        }

        [HttpGet("stores")]
        public async Task<ActionResult<IEnumerable<object>>> GetStores()
        {
            var response = await _getStoresHandler.ExecuteAsync();
            return Ok(response);
        }
    }
}
