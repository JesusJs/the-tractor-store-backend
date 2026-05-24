using Microsoft.AspNetCore.Mvc;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;

namespace TractorEcommerce.Api.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly CheckoutUseCase _checkoutUseCase; // Desde el módulo Cart para iniciar el flujo asíncrono
        private readonly GetOrderByIdUseCase _getOrderByIdUseCase; // Desde el módulo Order para lectura de DB
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            CheckoutUseCase checkoutUseCase,
            GetOrderByIdUseCase getOrderByIdUseCase,
            ILogger<OrderController> logger)
        {
            _checkoutUseCase = checkoutUseCase;
            _getOrderByIdUseCase = getOrderByIdUseCase;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder([FromBody] OrderPayloadDto payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.StoreId))
                throw new ArgumentException("Estructura de orden inválida. Datos obligatorios faltantes.");

            // Obtenemos el ID del carrito directamente desde las cookies de la petición HTTP actual
            if (!Request.Cookies.TryGetValue("tractor_session", out var cartId) || string.IsNullOrWhiteSpace(cartId))
            {
                _logger.LogWarning("Intento de Checkout rechazado: No se encontró cookie de sesión válida.");
                throw new InvalidOperationException("No existe una sesión de compra activa.");
            }

            _logger.LogInformation("Iniciando comando de checkout asíncrono para el carrito {CartId}", cartId);

            // Se dispara el caso de uso de Cart que publica en Kafka: checkout-requested-topic
            // El cliente recibe un 202 Accepted indicando que la orden entró a la cola de procesamiento.
            string customerId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            await _checkoutUseCase.ExecuteAsync(cartId, customerId);

            return Accepted(new { message = "La orden está siendo procesada.", sessionId = cartId });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El identificador de la orden es obligatorio.", nameof(id));

            _logger.LogInformation("Buscando recibo de la orden: {OrderId}", id);

            var receipt = await _getOrderByIdUseCase.ExecuteAsync(id);

            if (receipt == null)
            {
                _logger.LogWarning("Orden no localizada en la base de datos: {OrderId}", id);
                throw new DomainNotFoundException($"La orden con identificador {id} no existe.");
            }

            return Ok(receipt);
        }
    }

    public record OrderPayloadDto(string FirstName, string StoreId);
}
