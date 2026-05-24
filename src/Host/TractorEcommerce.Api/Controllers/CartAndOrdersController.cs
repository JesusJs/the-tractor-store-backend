using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Sales.Application.Handler;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CartAndOrdersController : ControllerBase
    {
        private readonly CheckoutCommandHandler _checkoutHandler;

        // Simulamos un Mock en memoria de persistencia rápida para la sesión de HOY
        private static readonly Dictionary<string, CartDto> ActiveCarts = new();

        public CartAndOrdersController(CheckoutCommandHandler checkoutHandler)
        {
            _checkoutHandler = checkoutHandler;
        }

        private string GetOrCreateUserSession()
        {
            // Requisito: Extraer de cabeceras o cookies. Si no viene, creamos una sesión temporal
            if (!Request.Headers.TryGetValue("X-Session-Id", out var sessionId))
            {
                sessionId = "session-anonima-123";
            }
            return sessionId!;
        }

        [HttpGet("cart")]
        public ActionResult<CartDto> GetCart()
        {
            var session = GetOrCreateUserSession();
            if (!ActiveCarts.TryGetValue(session, out var cart))
            {
                // Si no existe, devolvemos un carrito vacío estructurado correctamente
                cart = new CartDto(new List<CartItemDto>(), 0, 0, 0, 0);
            }
            return Ok(cart);
        }

        [HttpGet("cart/mini")]
        public ActionResult<MiniCartDto> GetMiniCart()
        {
            var session = GetOrCreateUserSession();
            int totalQuantity = ActiveCarts.TryGetValue(session, out var cart) ? cart.TotalItems : 0;
            return Ok(new MiniCartDto(totalQuantity)); // Endpoint 8 rápido
        }

        [HttpPost("cart/items")]
        public ActionResult<CartDto> AddToCart([FromBody] AddToCartRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
                return BadRequest(new { message = "SKU es requerido." }); // Error 400 exigido

            var session = GetOrCreateUserSession();

            // Simulación rápida de mutación del carrito
            // En código real, aquí invocarías a tu Agregado de Dominio 'Cart'
            var items = new List<CartItemDto>
        {
            new CartItemDto("tx-001", request.Sku, "Autonomous Titan", "GPS Edition", 85000, 1, "https://placehold.co/600x400")
        };

            var updatedCart = new CartDto(items, 1, 85000, 17850, 102850); // Simulación con IVA 21%
            ActiveCarts[session] = updatedCart;

            return Ok(updatedCart); // Retorna el carrito actualizado completo
        }

        [HttpPost("orders")]
        public async Task<ActionResult<OrderReceiptDto>> PlaceOrder([FromBody] OrderPayloadDto payload)
        {
            if (string.IsNullOrWhiteSpace(payload.FirstName) || string.IsNullOrWhiteSpace(payload.StoreId))
                return BadRequest(new { message = "Datos del payload inválidos." }); // Error 400

            var session = GetOrCreateUserSession();

            try
            {
                // Ejecutamos la lógica transaccional del caso de uso
                var receipt = await _checkoutHandler.ExecuteAsync(session, payload);
                return Ok(receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}