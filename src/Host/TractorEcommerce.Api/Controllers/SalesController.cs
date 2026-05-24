using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Sales.Application.Handler;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class SalesController : ControllerBase
    {
        private readonly CheckoutCommandHandler _checkoutHandler;
        private readonly AddToCartCommandHandler _addToCartHandler;
        private readonly RemoveFromCartCommandHandler _removeFromCartHandler;
        private readonly GetCartQueryHandler _getCartQueryHandler;
        private readonly GetMiniCartQueryHandler _getMiniCartQueryHandler;
        private readonly GetOrderByIdQueryHandler _getOrderByIdQueryHandler;
        private readonly ICatalogRepository _catalogRepository;

        public SalesController(
            CheckoutCommandHandler checkoutHandler,
            AddToCartCommandHandler addToCartHandler,
            RemoveFromCartCommandHandler removeFromCartHandler,
            GetCartQueryHandler getCartQueryHandler,
            GetMiniCartQueryHandler getMiniCartQueryHandler,
            GetOrderByIdQueryHandler getOrderByIdQueryHandler,
            ICatalogRepository catalogRepository)
        {
            _checkoutHandler = checkoutHandler;
            _addToCartHandler = addToCartHandler;
            _removeFromCartHandler = removeFromCartHandler;
            _getCartQueryHandler = getCartQueryHandler;
            _getMiniCartQueryHandler = getMiniCartQueryHandler;
            _getOrderByIdQueryHandler = getOrderByIdQueryHandler;
            _catalogRepository = catalogRepository;
        }

        private string GetOrCreateCartId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var nameIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(nameIdentifier))
                {
                    return nameIdentifier;
                }
            }

            if (Request.Cookies.TryGetValue("tractor_session", out var sessionId) && !string.IsNullOrWhiteSpace(sessionId))
            {
                return sessionId;
            }

            sessionId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Local HTTP development
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
            var cartDto = await _getCartQueryHandler.ExecuteAsync(cartId);
            return Ok(cartDto);
        }

        // 8. GET /api/cart/mini
        [HttpGet("cart/mini")]
        public async Task<ActionResult<MiniCartDto>> GetMiniCart()
        {
            var cartId = GetOrCreateCartId();
            var miniCart = await _getMiniCartQueryHandler.ExecuteAsync(cartId);
            return Ok(miniCart);
        }

        // 9. POST /api/cart/items
        [HttpPost("cart/items")]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                return BadRequest(new { message = "El SKU es mandatorio en el cuerpo de la petición." });

            var variant = await _catalogRepository.GetVariantBySkuAsync(request.Sku);
            if (variant == null)
                return NotFound(new { message = $"El SKU {request.Sku} no existe en el catálogo." });

            var product = await _catalogRepository.GetByIdAsync(variant.ProductId);
            if (product == null)
                return NotFound(new { message = $"El producto asociado al SKU {request.Sku} no existe." });

            var cartId = GetOrCreateCartId();
            var command = new AddToCartCommand(
                UserId: cartId,
                ProductId: product.Id,
                Sku: variant.Sku,
                ProductName: product.Name,
                VariantName: variant.name,
                Price: product.Price,
                Image: product.Image
            );

            var cartDto = await _addToCartHandler.ExecuteAsync(command);
            return Ok(cartDto);
        }

        // 10. DELETE /api/cart/items/{sku}
        [HttpDelete("cart/items/{sku}")]
        public async Task<ActionResult<CartDto>> RemoveFromCart(string sku)
        {
            var cartId = GetOrCreateCartId();
            var cartDto = await _removeFromCartHandler.ExecuteAsync(cartId, sku);
            return Ok(cartDto);
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
                var receipt = await _checkoutHandler.ExecuteAsync(cartId, payload);
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
            var receipt = await _getOrderByIdQueryHandler.ExecuteAsync(id);
            if (receipt == null)
            {
                return NotFound(new { message = $"La orden con identificador {id} no existe." });
            }
            return Ok(receipt);
        }
    }
}