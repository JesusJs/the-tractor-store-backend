using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Sales.Application.UseCase;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/cart")]
    public class CartController : ControllerBase
    {
        private readonly AddToCartUseCase _addToCartUseCase;
        private readonly RemoveFromCartUseCase _removeFromCartUseCase;
        private readonly GetCartUseCase _getCartUseCase;
        private readonly GetMiniCartUseCase _getMiniCartUseCase;
        private readonly ILogger<CartController> _logger;

        public CartController(
            AddToCartUseCase addToCartUseCase,
            RemoveFromCartUseCase removeFromCartUseCase,
            GetCartUseCase getCartUseCase,
            GetMiniCartUseCase miniCartUseCase,
            ILogger<CartController> logger)
        {
            _addToCartUseCase = addToCartUseCase;
            _removeFromCartUseCase = removeFromCartUseCase;
            _getCartUseCase = getCartUseCase;
            _getMiniCartUseCase = miniCartUseCase;
            _logger = logger;
        }

        private string GetOrCreateCartId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(nameId))
                    return nameId;
            }

            if (Request.Cookies.TryGetValue("tractor_session", out var sessionId) && !string.IsNullOrWhiteSpace(sessionId))
                return sessionId;

            sessionId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Cambiar a true en producción bajo HTTPS
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("tractor_session", sessionId, cookieOptions);

            _logger.LogInformation("Nueva sesión HttpOnly creada para carrito anónimo: {CartId}", sessionId);
            return sessionId;
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetCart()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _getCartUseCase.ExecuteAsync(cartId);
            return Ok(cart);
        }

        [HttpGet("mini")]
        public async Task<ActionResult<object>> GetMiniCart()
        {
            var cartId = GetOrCreateCartId();
            var miniCart = await _getMiniCartUseCase.ExecuteAsync(cartId);
            return Ok(miniCart);
        }

        [HttpPost("items")]
        public async Task<ActionResult<object>> AddToCart([FromBody] AddToCartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                throw new ArgumentException("El SKU es mandatorio en el cuerpo de la petición.", nameof(request.Sku));

            var cartId = GetOrCreateCartId();
            _logger.LogInformation("Añadiendo ítem al carrito {CartId}. SKU: {Sku}", cartId, request.Sku);

            var cart = await _addToCartUseCase.ExecuteAsync(cartId, request.Sku);
            return Ok(cart);
        }

        [HttpDelete("items/{sku}")]
        public async Task<ActionResult<object>> RemoveFromCart(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("El SKU es requerido para remover el ítem.", nameof(sku));

            var cartId = GetOrCreateCartId();
            _logger.LogInformation("Removiendo SKU {Sku} del carrito {CartId}", sku, cartId);

            var cart = await _removeFromCartUseCase.ExecuteAsync(cartId, sku);
            return Ok(cart);
        }
    }

    public record AddToCartRequest(string Sku);
}
