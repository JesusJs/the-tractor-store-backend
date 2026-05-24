using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.UseCase;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly CheckoutUseCase _checkoutUseCase;
        private readonly ILogger<OrderController> _logger;

        public OrderController(CheckoutUseCase checkoutUseCase, ILogger<OrderController> logger)
        {
            _checkoutUseCase = checkoutUseCase;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<OrderReceiptDto>> PlaceOrder([FromBody] OrderPayloadDto payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.StoreId))
                throw new ArgumentException("Estructura de orden inválida. Datos obligatorios faltantes.");

            // 1. LEER COOKIE: Intentamos obtener el token de sesión del carrito
            if (!Request.Cookies.TryGetValue("tractor_session", out var cartId) || string.IsNullOrWhiteSpace(cartId))
            {
                _logger.LogWarning("Intento de Checkout rechazado: No se encontró cookie de sesión 'tractor_session'.");
                throw new InvalidOperationException("No se puede procesar la orden porque no hay una sesión de compra activa.");
            }

            _logger.LogInformation("Iniciando procesamiento de checkout síncrono para el carrito/usuario: {CartId}", cartId);

            // Si el usuario está autenticado usamos su ID, de lo contrario usamos el 'cartId' (SessionId de la cookie HttpOnly)
            string userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? cartId;

            // 2. SOLUCIÓN AL ERROR: Invocamos la función con la firma exacta que me pasaste (string, OrderPayloadDto)
            var receipt = await _checkoutUseCase.ExecuteAsync(userId, payload);

            // Retornamos el recibo con un 200 OK tal como lo manejabas originalmente
            return Ok(receipt);
        }
    }
}
