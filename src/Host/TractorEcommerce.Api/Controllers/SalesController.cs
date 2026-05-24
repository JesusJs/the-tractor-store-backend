using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Sales.Application.Handler;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class SalesController : ControllerBase
    {
            private readonly CheckoutCommandHandler _checkoutHandler;

            // Simulación en memoria para resolver la persistencia del carrito de HOY de forma rápida
            private static readonly Dictionary<string, CartDto> MockCartDatabase = new();
            private static readonly Dictionary<string, OrderReceiptDto> MockOrderDatabase = new();

            public SalesController(CheckoutCommandHandler checkoutHandler)
            {
                _checkoutHandler = checkoutHandler;
            }

            private string GetOrCreateUserSession()
            {
                // El frontend puede enviar la sesión mediante cabeceras custom o cookies
                if (!Request.Headers.TryGetValue("X-Session-Id", out var sessionId))
                {
                    sessionId = "session-anonima-default";
                }
                return sessionId!;
            }

            // 7. GET /api/cart
            [HttpGet("cart")]
            public ActionResult<CartDto> GetCart()
            {
                var session = GetOrCreateUserSession();
                if (!MockCartDatabase.TryGetValue(session, out var cart))
                {
                    // Si el usuario no tiene carrito, devolvemos uno vacío estructurado
                    cart = new CartDto(new List<CartItemDto>(), 0, 0, 0, 0);
                }
                return Ok(cart);
            }

            // 8. GET /api/cart/mini
            [HttpGet("cart/mini")]
            public ActionResult<MiniCartDto> GetMiniCart()
            {
                var session = GetOrCreateUserSession();
                int totalQuantity = MockCartDatabase.TryGetValue(session, out var cart) ? cart.TotalItems : 0;
                return Ok(new MiniCartDto(totalQuantity));
            }

            // 9. POST /api/cart/items
            [HttpPost("cart/items")]
            public ActionResult<CartDto> AddToCart([FromBody] AddToCartRequest request)
            {
                if (string.IsNullOrWhiteSpace(request.Sku))
                    return BadRequest(new { message = "El SKU es mandatorio en el cuerpo de la petición." }); // Error 400

                var session = GetOrCreateUserSession();

                // Simulación de cálculo dinámico del 21% de IVA exigido por el contrato
                var itemPrice = 85000m;
                var quantity = 1;

                // Si ya existe el SKU en el mock, incrementamos la cantidad (+1) en lugar de duplicar el objeto
                if (MockCartDatabase.TryGetValue(session, out var existingCart))
                {
                    // Lógica de incremento simulada para el test rápido de hoy
                    quantity = existingCart.TotalItems + 1;
                }

                var subTotal = itemPrice * quantity;
                var tax = System.Math.Round(subTotal * 0.21m, 2);
                var total = subTotal + tax;

                var items = new List<CartItemDto>
            {
                new CartItemDto("tx-001", request.Sku, "Autonomous Titan", "GPS Guided System", itemPrice, quantity, "https://placehold.co/600x400")
            };

                var updatedCart = new CartDto(items, quantity, subTotal, tax, total);
                MockCartDatabase[session] = updatedCart;

                return Ok(updatedCart);
            }

            // 10. DELETE /api/cart/items/{sku}
            [HttpDelete("cart/items/{sku}")]
            public ActionResult<CartDto> RemoveFromCart(string sku)
            {
                var session = GetOrCreateUserSession();

                // Vaciamos o alteramos el carrito simulado para el frontend
                var emptyCart = new CartDto(new List<CartItemDto>(), 0, 0, 0, 0);
                MockCartDatabase[session] = emptyCart;

                return Ok(emptyCart);
            }

            // 11. POST /api/orders
            [HttpPost("orders")]
            public async Task<ActionResult<OrderReceiptDto>> PlaceOrder([FromBody] OrderPayloadDto payload)
            {
                if (string.IsNullOrWhiteSpace(payload.FirstName) || string.IsNullOrWhiteSpace(payload.StoreId))
                    return BadRequest(new { message = "Estructura de orden inválida. Faltan datos obligatorios." }); // Error 400

                var session = GetOrCreateUserSession();

                try
                {
                    // En producción invoca al handler transaccional. Para velocidad de prueba hoy:
                    var receipt = await _checkoutHandler.ExecuteAsync(session, payload);

                    // Guardamos en nuestro mock para que el GET /api/orders/{id} funcione inmediatamente
                    MockOrderDatabase[receipt.Id] = receipt;

                    // Limpiamos el carrito local de la memoria
                    MockCartDatabase.Remove(session);

                    return Ok(receipt);
                }
                catch (System.InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // 12. GET /api/orders/{id}
            [HttpGet("orders/{id}")]
            public ActionResult<OrderReceiptDto> GetOrder(string id)
            {
                if (!MockOrderDatabase.TryGetValue(id, out var receipt))
                {
                    return NotFound(new { message = $"La orden con identificador {id} no existe." }); // Error 404
                }
                return Ok(receipt);
            }
    }
 }