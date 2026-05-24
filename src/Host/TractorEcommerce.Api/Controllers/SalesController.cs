using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class SalesController : ControllerBase
    {
        private readonly CheckoutUseCase _checkoutUseCase;
        private readonly AddToCartUseCase _addToCartUseCase;
        private readonly RemoveFromCartUseCase _removeFromCartUseCase;
        private readonly GetCartUseCase _getCartUseCase;
        private readonly GetMiniCartUseCase _getMiniCartUseCase;
        private readonly GetOrderByIdUseCase _getOrderByIdUseCase;

        public SalesController(
            CheckoutUseCase checkoutUseCase,
            AddToCartUseCase addToCartUseCase,
            RemoveFromCartUseCase removeFromCartUseCase,
            GetCartUseCase getCartUseCase,
            GetMiniCartUseCase getMiniCartUseCase,
            GetOrderByIdUseCase getOrderByIdUseCase)
        {
            _checkoutUseCase = checkoutUseCase;
            _addToCartUseCase = addToCartUseCase;
            _removeFromCartUseCase = removeFromCartUseCase;
            _getCartUseCase = getCartUseCase;
            _getMiniCartUseCase = getMiniCartUseCase;
            _getOrderByIdUseCase = getOrderByIdUseCase;
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
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("tractor_session", sessionId, cookieOptions);
            return sessionId;
        }

        // 7. GET /api/cart
        [HttpGet("cart")]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var cartId = GetOrCreateCartId();
            var cart = await _getCartUseCase.ExecuteAsync(cartId);
            return Ok(cart);
        }

        // 8. GET /api/cart/mini
        [HttpGet("cart/mini")]
        public async Task<ActionResult<MiniCartDto>> GetMiniCart()
        {
            var cartId = GetOrCreateCartId();
            var miniCart = await _getMiniCartUseCase.ExecuteAsync(cartId);
            return Ok(miniCart);
        }

        // 9. POST /api/cart/items
        [HttpPost("cart/items")]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                return BadRequest(new { message = "El SKU es mandatorio en el cuerpo de la petición." });

            var cartId = GetOrCreateCartId();
            var command = new AddToCartCommand(cartId, request.Sku);

            try
            {
                var cart = await _addToCartUseCase.ExecuteAsync(command);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // 10. DELETE /api/cart/items/{sku}
        [HttpDelete("cart/items/{sku}")]
        public async Task<ActionResult<CartDto>> RemoveFromCart(string sku)
        {
            var cartId = GetOrCreateCartId();
            var cart = await _removeFromCartUseCase.ExecuteAsync(cartId, sku);
            return Ok(cart);
        }

        // 11. POST /api/orders
        [HttpPost("orders")]
        public async Task<ActionResult<OrderReceiptDto>> PlaceOrder([FromBody] OrderPayloadDto payload)
        {
            if (string.IsNullOrWhiteSpace(payload.FirstName) || string.IsNullOrWhiteSpace(payload.StoreId))
                return BadRequest(new { message = "Estructura de orden inválida. Faltan datos obligatorios." });

            var cartId = GetOrCreateCartId();
            try
            {
                var receipt = await _checkoutUseCase.ExecuteAsync(cartId, payload);
                return Ok(receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 12. GET /api/orders/{id}
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<OrderReceiptDto>> GetOrder(string id)
        {
            var receipt = await _getOrderByIdUseCase.ExecuteAsync(id);
            if (receipt == null)
                return NotFound(new { message = $"La orden con identificador {id} no existe." });
            return Ok(receipt);
        }
    }
}