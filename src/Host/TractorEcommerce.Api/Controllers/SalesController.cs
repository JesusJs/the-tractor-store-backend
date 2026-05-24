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
        private readonly ISalesRepository _salesRepository;
        private readonly ICatalogRepository _catalogRepository;

        public SalesController(
            CheckoutCommandHandler checkoutHandler,
            ISalesRepository salesRepository,
            ICatalogRepository catalogRepository)
        {
            _checkoutHandler = checkoutHandler;
            _salesRepository = salesRepository;
            _catalogRepository = catalogRepository;
        }

        private string GetOrCreateUserSession()
        {
            if (!Request.Cookies.TryGetValue("tractor_session", out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Local HTTP development
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("tractor_session", sessionId, cookieOptions);
            }
            return sessionId;
        }

        private CartDto MapToCartDto(Cart cart)
        {
            var itemDtos = cart.Items.Select(i => new CartItemDto(
                ProductId: i.ProductId,
                VariantId: i.VariantId,
                ProductName: i.ProductName,
                VariantName: i.VariantName,
                Price: i.Price,
                Quantity: i.Quantity,
                Image: i.Image
            )).ToList();

            return new CartDto(
                Items: itemDtos,
                TotalItems: cart.TotalItems,
                SubTotal: cart.SubTotal,
                Tax: cart.Tax,
                Total: cart.Total
            );
        }

        // 7. GET /api/cart
        [HttpGet("cart")]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var session = GetOrCreateUserSession();
            var cart = await _salesRepository.GetCartByUserIdAsync(session);
            if (cart == null)
            {
                cart = new Cart(session);
                await _salesRepository.SaveCartAsync(cart);
            }
            return Ok(MapToCartDto(cart));
        }

        // 8. GET /api/cart/mini
        [HttpGet("cart/mini")]
        public async Task<ActionResult<MiniCartDto>> GetMiniCart()
        {
            var session = GetOrCreateUserSession();
            var cart = await _salesRepository.GetCartByUserIdAsync(session);
            int totalQuantity = cart?.TotalItems ?? 0;
            return Ok(new MiniCartDto(totalQuantity));
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

            var session = GetOrCreateUserSession();
            var cart = await _salesRepository.GetCartByUserIdAsync(session);
            if (cart == null)
            {
                cart = new Cart(session);
            }

            cart.AddItem(
                productId: product.Id,
                sku: variant.Sku,
                productName: product.Name,
                variantName: variant.name,
                price: product.Price,
                image: product.Image
            );

            await _salesRepository.SaveCartAsync(cart);
            return Ok(MapToCartDto(cart));
        }

        // 10. DELETE /api/cart/items/{sku}
        [HttpDelete("cart/items/{sku}")]
        public async Task<ActionResult<CartDto>> RemoveFromCart(string sku)
        {
            var session = GetOrCreateUserSession();
            var cart = await _salesRepository.GetCartByUserIdAsync(session);
            if (cart != null)
            {
                cart.RemoveItem(sku);
                await _salesRepository.SaveCartAsync(cart);
            }
            else
            {
                cart = new Cart(session);
            }
            return Ok(MapToCartDto(cart));
        }

        // 11. POST /api/orders
        [HttpPost("orders")]
        public async Task<ActionResult<OrderReceiptDto>> PlaceOrder([FromBody] OrderPayloadDto payload)
        {
            if (string.IsNullOrWhiteSpace(payload.FirstName) || string.IsNullOrWhiteSpace(payload.StoreId))
                return BadRequest(new { message = "Estructura de orden inválida. Faltan datos obligatorios." });

            var session = GetOrCreateUserSession();
            var cart = await _salesRepository.GetCartByUserIdAsync(session);
            if (cart == null || !cart.Items.Any())
                return BadRequest(new { message = "No se puede crear un pedido desde un carrito vacío." });

            try
            {
                var receipt = await _checkoutHandler.ExecuteAsync(session, payload);
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
            var receipt = await _salesRepository.GetOrderByIdAsync(id);
            if (receipt == null)
            {
                return NotFound(new { message = $"La orden con identificador {id} no existe." });
            }
            return Ok(receipt);
        }
    }
}