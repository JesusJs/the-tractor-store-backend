using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Domain.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;
using OrderPlacedEvent = TractorEcommerce.Modules.Sales.Domain.Events.OrderPlacedEvent;

namespace TractorEcommerce.Modules.Sales.Application.UseCase
{
    public class CheckoutUseCase
    {
        private readonly ISalesRepository _salesRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IEventBus _eventBus;

        public CheckoutUseCase(ISalesRepository salesRepository, IInventoryService inventoryService, IEventBus eventBus)
        {
            _salesRepository = salesRepository;
            _inventoryService = inventoryService;
            _eventBus = eventBus;
        }

        public async Task<OrderReceiptDto> ExecuteAsync(string userId, OrderPayloadDto payload)
        {
            // 1. Obtener el carrito actual del usuario
            var cart = await _salesRepository.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("El carrito está vacío.");

            // Iniciamos la transacción atómica
            var transaction = await _salesRepository.BeginTransactionAsync();
            try
            {
                // 2. Verificar y mutar stock de cada variante
                foreach (var item in cart.Items)
                {
                    bool hasStock = await _inventoryService.DecreaseStockAsync(item.VariantId, item.Quantity);
                    if (!hasStock)
                        throw new InvalidOperationException($"Stock insuficiente para el SKU: {item.VariantId}");
                }

                // 3. Crear el registro de la Orden con los totales congelados del carrito
                var orderId = $"ORD-{new Random().Next(100000, 999999)}"; // Generación de ID único

                // Mapeamos los items del carrito para adjuntarlos a la respuesta/orden
                var orderItemsDto = cart.Items.Select(i => new CartItemDto(
                    i.ProductId, i.VariantId, i.ProductName, i.VariantName, i.Price, i.Quantity, i.Image
                )).ToList();

                var receipt = new OrderReceiptDto(
                    Id: orderId,
                    FirstName: payload.FirstName,
                    LastName: payload.LastName,
                    StoreId: payload.StoreId,
                    ExtraPickups: payload.ExtraPickups,
                    Items: orderItemsDto,
                    SubTotal: cart.SubTotal,
                    Tax: cart.Tax,
                    Total: cart.Total,
                    PlacedAt: DateTime.UtcNow
                );

                // 4. Guardar la orden en persistencia
                await _salesRepository.SaveOrderReceiptAsync(receipt);

                // Mapeamos los items para el evento de Kafka antes de vaciar el carrito
                var eventItems = cart.Items.Select(i => new Domain.Events.OrderEventItem(i.VariantId, i.Quantity)).ToList();

                // 5. Vaciar el carrito del usuario
                cart.Clear();
                await _salesRepository.SaveCartAsync(cart);

                // Confirmar cambios en la base de datos (Commit)
                await transaction.CommitAsync();

                var orderPlacedEvent = new OrderPlacedEvent(orderId, eventItems, DateTime.UtcNow);
                // Publicamos en Kafka de forma asíncrona y desacoplada
                await _eventBus.PublishAsync(
                    topic: "sales.orders.placed",
                    key: orderId,
                    message: orderPlacedEvent
                );
                return receipt;
            }
            catch
            {
                // Si algo falla, revertimos todo (Rollback) para evitar inconsistencias de inventario
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
