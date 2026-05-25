using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Inventory.Application.UseCase;
using TractorEcommerce.Modules.Inventory.Domain.Entities;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Inventory.Tests.Application
{
    // =========================================================================
    // GetInventoryStatusUseCase Tests
    // =========================================================================
    public class GetInventoryStatusUseCaseTests
    {
        private readonly IInventoryRepository _repository;
        private readonly GetInventoryStatusUseCase _useCase;

        public GetInventoryStatusUseCaseTests()
        {
            _repository = Substitute.For<IInventoryRepository>();
            _useCase = new GetInventoryStatusUseCase(_repository);
        }

        [Fact]
        public async Task Execute_WithExistingSku_ReturnsInventoryStatusDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = new InventoryItem(id, "TX-001", 42);
            _repository.GetBySkuAsync("TX-001").Returns(item);

            // Act
            var result = await _useCase.ExecuteAsync("TX-001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TX-001", result!.Sku);
            Assert.Equal(42, result.Quantity);
        }

        [Fact]
        public async Task Execute_WithMissingSku_ReturnsNull()
        {
            // Arrange
            _repository.GetBySkuAsync("MISSING").Returns((InventoryItem?)null);

            // Act
            var result = await _useCase.ExecuteAsync("MISSING");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Execute_CallsRepositoryWithCorrectSku()
        {
            // Arrange
            var sku = "TX-XYZ";
            _repository.GetBySkuAsync(sku).Returns((InventoryItem?)null);

            // Act
            await _useCase.ExecuteAsync(sku);

            // Assert
            await _repository.Received(1).GetBySkuAsync(sku);
        }

        [Fact]
        public async Task Execute_WithZeroStock_ReturnsZeroQuantity()
        {
            // Arrange
            var item = new InventoryItem(Guid.NewGuid(), "TX-ZERO", 0);
            _repository.GetBySkuAsync("TX-ZERO").Returns(item);

            // Act
            var result = await _useCase.ExecuteAsync("TX-ZERO");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result!.Quantity);
        }

        [Fact]
        public async Task Execute_WithHighStock_ReturnsCorrectQuantity()
        {
            // Arrange
            var item = new InventoryItem(Guid.NewGuid(), "TX-HIGH", 999);
            _repository.GetBySkuAsync("TX-HIGH").Returns(item);

            // Act
            var result = await _useCase.ExecuteAsync("TX-HIGH");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999, result!.Quantity);
        }
    }

    // =========================================================================
    // DeductStockOnOrderPlacedUseCase Tests
    // =========================================================================
    public class DeductStockOnOrderPlacedUseCaseTests
    {
        private readonly IInventoryRepository _repository;
        private readonly ILogger<DeductStockOnOrderPlacedUseCase> _logger;
        private readonly DeductStockOnOrderPlacedUseCase _useCase;

        public DeductStockOnOrderPlacedUseCaseTests()
        {
            _repository = Substitute.For<IInventoryRepository>();
            _logger = Substitute.For<ILogger<DeductStockOnOrderPlacedUseCase>>();
            _useCase = new DeductStockOnOrderPlacedUseCase(_repository, _logger);
        }

        private static OrderPlacedEvent BuildEvent(string orderId, params (string Sku, int Quantity, decimal Price)[] items)
        {
            var cartItems = items.Select(i => new CartItemDto(i.Sku, i.Quantity, i.Price)).ToList();
            return new OrderPlacedEvent(Guid.NewGuid(), "customer-1", cartItems, DateTime.UtcNow);
        }

        [Fact]
        public async Task HandleAsync_WithValidItem_DeductsStockAndUpdates()
        {
            // Arrange
            var inventoryItem = new InventoryItem(Guid.NewGuid(), "TX-001", 20);
            _repository.GetBySkuAsync("TX-001").Returns(inventoryItem);

            var @event = BuildEvent("ORD-001", ("TX-001", 5, 100m));

            // Act
            await _useCase.HandleAsync(@event);

            // Assert
            Assert.Equal(15, inventoryItem.AvailableStock);
            await _repository.Received(1).UpdateAsync(inventoryItem);
        }

        [Fact]
        public async Task HandleAsync_WithMultipleItems_DeductsAllCorrectly()
        {
            // Arrange
            var item1 = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            var item2 = new InventoryItem(Guid.NewGuid(), "TX-002", 8);
            _repository.GetBySkuAsync("TX-001").Returns(item1);
            _repository.GetBySkuAsync("TX-002").Returns(item2);

            var @event = BuildEvent("ORD-002", ("TX-001", 3, 100m), ("TX-002", 2, 200m));

            // Act
            await _useCase.HandleAsync(@event);

            // Assert
            Assert.Equal(7, item1.AvailableStock);
            Assert.Equal(6, item2.AvailableStock);
            await _repository.Received(1).UpdateAsync(item1);
            await _repository.Received(1).UpdateAsync(item2);
        }

        [Fact]
        public async Task HandleAsync_WithMissingSku_ThrowsInvalidOperationException()
        {
            // Arrange
            _repository.GetBySkuAsync(Arg.Any<string>()).Returns((InventoryItem?)null);

            var @event = BuildEvent("ORD-003", ("GHOST", 1, 50m));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.HandleAsync(@event));
        }

        [Fact]
        public async Task HandleAsync_WithInsufficientStock_ThrowsInvalidOperationException()
        {
            // Arrange
            var inventoryItem = new InventoryItem(Guid.NewGuid(), "TX-LOW", 2);
            _repository.GetBySkuAsync("TX-LOW").Returns(inventoryItem);

            var @event = BuildEvent("ORD-004", ("TX-LOW", 10, 100m));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.HandleAsync(@event));
        }

        [Fact]
        public async Task HandleAsync_WhenRepositoryThrows_RethrowsException()
        {
            // Arrange
            _repository.GetBySkuAsync(Arg.Any<string>())
                .Returns<InventoryItem?>(_ => throw new Exception("DB error"));

            var @event = BuildEvent("ORD-005", ("TX-001", 1, 100m));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _useCase.HandleAsync(@event));
        }

        [Fact]
        public async Task HandleAsync_ExactStock_DeductsToZero()
        {
            // Arrange
            var inventoryItem = new InventoryItem(Guid.NewGuid(), "TX-EXACT", 5);
            _repository.GetBySkuAsync("TX-EXACT").Returns(inventoryItem);

            var @event = BuildEvent("ORD-006", ("TX-EXACT", 5, 100m));

            // Act
            await _useCase.HandleAsync(@event);

            // Assert
            Assert.Equal(0, inventoryItem.AvailableStock);
        }
    }
}
