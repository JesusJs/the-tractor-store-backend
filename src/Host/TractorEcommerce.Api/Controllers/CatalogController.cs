using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Api.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly GetHomeTeasersUseCase _getHomeTeasers;
        private readonly GetCatalogCategoryUseCase _getCatalogCategory;
        private readonly GetProductDetailUseCase _getProductDetail;
        private readonly GetRecommendationsUseCase _getRecommendations;
        private readonly GetStoresUseCase _getStores;

        public CatalogController(
            GetHomeTeasersUseCase getHomeTeasers,
            GetCatalogCategoryUseCase getCatalogCategory,
            GetProductDetailUseCase getProductDetail,
            GetRecommendationsUseCase getRecommendations,
            GetStoresUseCase getStores)
        {
            _getHomeTeasers = getHomeTeasers;
            _getCatalogCategory = getCatalogCategory;
            _getProductDetail = getProductDetail;
            _getRecommendations = getRecommendations;
            _getStores = getStores;
        }

        [HttpGet("home")]
        public async Task<ActionResult<IEnumerable<TeaserDto>>> GetHome()
        {
            var teasers = await _getHomeTeasers.ExecuteAsync();
            return Ok(teasers);
        }

        [HttpGet("categories/{filter}")]
        public async Task<ActionResult<CatalogCategoryDto>> GetCategory(string filter)
        {
            // VALIDACIÓN 400: Si viene vacío, lanzamos ArgumentException. El middleware responderá un 400 estructurado.
            if (string.IsNullOrWhiteSpace(filter))
                throw new ArgumentException("El filtro de categoría no puede estar vacío.", nameof(filter));

            var result = await _getCatalogCategory.ExecuteAsync(filter);
            return Ok(result);
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProduct(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El ID del producto es obligatorio.", nameof(id));

            var detail = await _getProductDetail.ExecuteAsync(id);

            // CORRECTO (404): En vez de usar return NotFound(), lanzamos nuestra excepción de dominio.
            // El GlobalExceptionMiddleware la atrapará y devolverá el JSON estructurado con código NOT_FOUND.
            if (detail == null)
                throw new DomainNotFoundException($"El tractor con ID '{id}' no existe en el catálogo.");

            return Ok(detail);
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<IEnumerable<ProductItemDto>>> GetRecommendations([FromQuery] string? skus)
        {
            var dtos = await _getRecommendations.ExecuteAsync(skus);
            return Ok(dtos);
        }

        [HttpGet("stores")]
        public async Task<ActionResult<IEnumerable<object>>> GetStores()
        {
            var result = await _getStores.ExecuteAsync();
            return Ok(result);
        }
    }
}