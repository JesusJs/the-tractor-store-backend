using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Cart.Application.Commands;
using TractorEcommerce.Modules.Cart.Application.UseCase;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/cart")]
    public class CartController : ControllerBase
        {
            private readonly AddToCartUseCase _addToCartUseCase;
            private readonly RemoveFromCartUseCase _removeFromCartUseCase;
            private readonly GetCartUseCase _getCartUseCase;
            private readonly ILogger<CartController> _logger;

            public CartController(
                AddToCartUseCase addToCartUseCase,
                RemoveFromCartUseCase removeFromCartUseCase,
                GetCartUseCase getCartUseCase,
                ILogger<CartController> logger)
            {
                _addToCartUseCase = addToCartUseCase;
                _removeFromCartUseCase = removeFromCartUseCase;
                _getCartUseCase = getCartUseCase;
                _logger = logger;
            }

            private string GetOrCreateCartId()
            {
                if (User?.Identity?.IsAuthenticated == true)
                {
                    var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrWhiteSpace(nameId))
                        return nameId;
                }

                if (Request.Cookies.TryGetValue("tractor_session", out var sessionId) && !string.IsNullOrWhiteSpace(sessionId))
                    return sessionId;

                sessionId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
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

            [HttpPost("items")]
            public async Task<ActionResult<object>> AddToCart([FromBody] AddToCartRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Sku))
                    throw new ArgumentException("El SKU es mandatorio en el cuerpo de la petición.", nameof(request.Sku));

                var cartId = GetOrCreateCartId();
                _logger.LogInformation("Añadiendo ítem al carrito {CartId}. SKU: {Sku}", cartId, request.Sku);

                // Armamos el comando con los datos de entrada
                // Nota: Aquí pasamos cantidad 1 y precio simulado temporal (ej: 250000) o lo que mande tu catálogo
                var command = new AddToCartCommand(cartId, request.Sku, 1, request.Price);

                var cart = await _addToCartUseCase.ExecuteAsync(command);
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

        public record AddToCartRequest(string Sku, decimal Price);
}
