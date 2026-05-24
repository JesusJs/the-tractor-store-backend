using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using TractorEcommerce.Modules.Sales.Application.Interfaces;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using TractorEcommerce.Modules.Shared.Application.Events;
using Xunit;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Tests.Application
{
    // ---------------------------------------------------------------------------
    // GetCartUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetCartUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly GetCartUseCase _useCase;

        public GetCartUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _useCase = new GetCartUseCase(_salesRepository);
        }

        [Fact]
        public async Task Execute_WithExistingCart_ReturnsMappedDto()
        {
            // Arrange
            var userId = "user-1";
            var cart = new Cart(userId);
            cart.AddItem("p-1", "SKU-A", "Tractor A", "Standard", 10000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            // Act
            var result = await _useCase.ExecuteAsync(userId);

            // Assert
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(10000, result.SubTotal);
            Assert.Equal(2100, result.Tax);  // 10000 * 0.21
            Assert.Equal(12100, result.Total);
        }

        [Fact]
        public async Task Execute_WithNoCart_CreatesAndSavesEmptyCart()
        {
            // Arrange
            var userId = "new-user";
            _salesRepository.GetCartByUserIdAsync(userId).Returns((Cart?)null);

            // Act
            var result = await _useCase.ExecuteAsync(userId);

            // Assert
            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Items);
            await _salesRepository.Received(1).SaveCartAsync(Arg.Is<Cart>(c => c.UserId == userId));
        }
    }

    // ---------------------------------------------------------------------------
    // AddToCartUseCase Tests
    // ---------------------------------------------------------------------------
    public class AddToCartUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly ICatalogService _catalogService;
        private readonly AddToCartUseCase _useCase;

        public AddToCartUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _catalogService = Substitute.For<ICatalogService>();
            _useCase = new AddToCartUseCase(_salesRepository, _catalogService);
        }

        [Fact]
        public async Task Execute_WithValidSku_AddsItemAndReturnsCart()
        {
            // Arrange
            var userId = "user-1";
            var sku = "SKU-VALID";
            var productInfo = new CatalogProductInfo("p-1", sku, "Tractor A", "Standard", 8500, "img");
            _catalogService.GetProductBySkuAsync(sku).Returns(productInfo);

            var cart = new Cart(userId);
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            // Act
            var result = await _useCase.ExecuteAsync(new AddToCartCommand(userId, sku));

            // Assert
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(8500, result.SubTotal);
            await _salesRepository.Received(1).SaveCartAsync(Arg.Any<Cart>());
        }

        [Fact]
        public async Task Execute_WithInvalidSku_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = "user-1";
            var sku = "MISSING-SKU";
            _catalogService.GetProductBySkuAsync(sku).Returns((CatalogProductInfo?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _useCase.ExecuteAsync(new AddToCartCommand(userId, sku)));
        }

        [Fact]
        public async Task Execute_SameSkuTwice_IncrementsQuantity()
        {
            // Arrange
            var userId = "user-1";
            var sku = "SKU-DUP";
            var productInfo = new CatalogProductInfo("p-1", sku, "Tractor B", "GPS", 5000, "img");
            _catalogService.GetProductBySkuAsync(sku).Returns(productInfo);

            var cart = new Cart(userId);
            cart.AddItem("p-1", sku, "Tractor B", "GPS", 5000, "img"); // pre-existing item
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            // Act
            var result = await _useCase.ExecuteAsync(new AddToCartCommand(userId, sku));

            // Assert — quantity increments, still 1 distinct item
            Assert.Equal(2, result.TotalItems);
            Assert.Single(result.Items);
            Assert.Equal(10000, result.SubTotal);
        }

        [Fact]
        public async Task Execute_WithNoExistingCart_CreatesNewCart()
        {
            // Arrange
            var userId = "brand-new";
            var sku = "SKU-NEW";
            var productInfo = new CatalogProductInfo("p-2", sku, "Tractor C", "Auto", 12000, "img");
            _catalogService.GetProductBySkuAsync(sku).Returns(productInfo);
            _salesRepository.GetCartByUserIdAsync(userId).Returns((Cart?)null);

            // Act
            var result = await _useCase.ExecuteAsync(new AddToCartCommand(userId, sku));

            // Assert
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(12000, result.SubTotal);
        }
    }

    // ---------------------------------------------------------------------------
    // RemoveFromCartUseCase Tests
    // ---------------------------------------------------------------------------
    public class RemoveFromCartUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly RemoveFromCartUseCase _useCase;

        public RemoveFromCartUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _useCase = new RemoveFromCartUseCase(_salesRepository);
        }

        [Fact]
        public async Task Execute_WithExistingItem_RemovesAndSaves()
        {
            // Arrange
            var userId = "user-1";
            var sku = "SKU-DEL";
            var cart = new Cart(userId);
            cart.AddItem("p-1", sku, "Tractor A", "STD", 9000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            // Act
            var result = await _useCase.ExecuteAsync(userId, sku);

            // Assert
            Assert.Equal(0, result.TotalItems);
            await _salesRepository.Received(1).SaveCartAsync(Arg.Any<Cart>());
        }

        [Fact]
        public async Task Execute_WithNoCart_ReturnsEmptyCart()
        {
            // Arrange
            var userId = "ghost-user";
            _salesRepository.GetCartByUserIdAsync(userId).Returns((Cart?)null);

            // Act
            var result = await _useCase.ExecuteAsync(userId, "ANY-SKU");

            // Assert
            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task Execute_RemovingNonExistentSku_LeavesCartUnchanged()
        {
            // Arrange
            var userId = "user-2";
            var cart = new Cart(userId);
            cart.AddItem("p-1", "SKU-KEEP", "Tractor B", "GPS", 7000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            // Act
            var result = await _useCase.ExecuteAsync(userId, "SKU-MISSING");

            // Assert
            Assert.Equal(1, result.TotalItems);
        }
    }

    // ---------------------------------------------------------------------------
    // CheckoutUseCase Tests
    // ---------------------------------------------------------------------------
    public class CheckoutUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IEventBus _eventBus;
        private readonly CheckoutUseCase _useCase;

        public CheckoutUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _inventoryService = Substitute.For<IInventoryService>();
            _eventBus = Substitute.For<IEventBus>();
            _useCase = new CheckoutUseCase(_salesRepository, _inventoryService, _eventBus);
        }

        private IDbTransactionWrapper BuildTransaction()
        {
            var tx = Substitute.For<IDbTransactionWrapper>();
            return tx;
        }

        [Fact]
        public async Task Execute_WithValidCart_ReturnsReceipt()
        {
            // Arrange
            var userId = "user-1";
            var cart = new Cart(userId);
            cart.AddItem("p-1", "SKU-A", "Tractor A", "STD", 10000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);
            _salesRepository.BeginTransactionAsync().Returns(BuildTransaction());
            _inventoryService.DecreaseStockAsync("SKU-A", 1).Returns(true);

            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string>());

            // Act
            var receipt = await _useCase.ExecuteAsync(userId, payload);

            // Assert
            Assert.Equal("John", receipt.FirstName);
            Assert.Equal("Doe", receipt.LastName);
            Assert.Equal("store-1", receipt.StoreId);
            Assert.Equal(10000, receipt.SubTotal);
            Assert.Equal(2100, receipt.Tax);
            Assert.Equal(12100, receipt.Total);
        }

        [Fact]
        public async Task Execute_WithEmptyCart_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "empty-user";
            _salesRepository.GetCartByUserIdAsync(userId).Returns((Cart?)null);

            var payload = new OrderPayloadDto("Jane", "Doe", "store-2", new List<string>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _useCase.ExecuteAsync(userId, payload));
        }

        [Fact]
        public async Task Execute_WithInsufficientStock_RollsBackAndThrows()
        {
            // Arrange
            var userId = "user-3";
            var cart = new Cart(userId);
            cart.AddItem("p-2", "SKU-B", "Tractor B", "GPS", 15000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);

            var tx = BuildTransaction();
            _salesRepository.BeginTransactionAsync().Returns(tx);
            _inventoryService.DecreaseStockAsync("SKU-B", 1).Returns(false); // no stock

            var payload = new OrderPayloadDto("John", "Smith", "store-3", new List<string>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _useCase.ExecuteAsync(userId, payload));

            await tx.Received(1).RollbackAsync();
        }

        [Fact]
        public async Task Execute_Success_ClearsCartAndPublishesEvent()
        {
            // Arrange
            var userId = "user-4";
            var cart = new Cart(userId);
            cart.AddItem("p-3", "SKU-C", "Tractor C", "Auto", 20000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);
            _salesRepository.BeginTransactionAsync().Returns(BuildTransaction());
            _inventoryService.DecreaseStockAsync("SKU-C", 1).Returns(true);

            var payload = new OrderPayloadDto("Maria", "Lopez", "store-4", new List<string>());

            // Act
            var receipt = await _useCase.ExecuteAsync(userId, payload);

            // Assert — cart is saved (cleared) and event is published
            await _salesRepository.Received(1).SaveCartAsync(Arg.Any<Cart>());
            await _eventBus.Received(1).PublishAsync(
                "sales.orders.placed",
                Arg.Any<string>(),
                Arg.Any<object>());
        }

        [Fact]
        public async Task Execute_Success_GeneratesOrderIdWithOrdPrefix()
        {
            // Arrange
            var userId = "user-5";
            var cart = new Cart(userId);
            cart.AddItem("p-4", "SKU-D", "Tractor D", "STD", 30000, "img");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(cart);
            _salesRepository.BeginTransactionAsync().Returns(BuildTransaction());
            _inventoryService.DecreaseStockAsync("SKU-D", 1).Returns(true);

            var payload = new OrderPayloadDto("Pedro", "García", "store-5", new List<string>());

            // Act
            var receipt = await _useCase.ExecuteAsync(userId, payload);

            // Assert
            Assert.StartsWith("ORD-", receipt.Id);
        }
    }

    // ---------------------------------------------------------------------------
    // GetOrderByIdUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetOrderByIdUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly GetOrderByIdUseCase _useCase;

        public GetOrderByIdUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _useCase = new GetOrderByIdUseCase(_salesRepository);
        }

        [Fact]
        public async Task Execute_WithExistingOrder_ReturnsReceipt()
        {
            // Arrange
            var orderId = "ORD-999888";
            var receipt = new OrderReceiptDto(orderId, "Ana", "Ruiz", "store-1",
                new List<string>(), new List<CartItemDto>(), 5000, 1050, 6050, DateTime.UtcNow);
            _salesRepository.GetOrderByIdAsync(orderId).Returns(receipt);

            // Act
            var result = await _useCase.ExecuteAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result!.Id);
            Assert.Equal("Ana", result.FirstName);
        }

        [Fact]
        public async Task Execute_WithMissingOrder_ReturnsNull()
        {
            // Arrange
            _salesRepository.GetOrderByIdAsync("ORD-000000").Returns((OrderReceiptDto?)null);

            // Act
            var result = await _useCase.ExecuteAsync("ORD-000000");

            // Assert
            Assert.Null(result);
        }
    }
}
