using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using TractorEcommerce.Modules.Sales.Application.Interfaces;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;
using Xunit;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

// Alias to avoid ambiguity with TractorEcommerce.Modules.Cart namespace
using SalesCart = TractorEcommerce.Modules.Sales.Domain.Entities.Cart;
// Alias to avoid ambiguity between Sales and Shared OrderPlacedEvent
using SalesOrderPlacedEvent = TractorEcommerce.Modules.Sales.Domain.Events.OrderPlacedEvent;

namespace TractorEcommerce.Modules.Sales.Tests.Application
{
    public class SalesModuleApplicationUseCaseTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly ICatalogService _catalogService;
        private readonly IInventoryService _inventoryService;
        private readonly IEventBus _eventBus;

        public SalesModuleApplicationUseCaseTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _catalogService = Substitute.For<ICatalogService>();
            _inventoryService = Substitute.For<IInventoryService>();
            _eventBus = Substitute.For<IEventBus>();
        }

        // ==========================================
        // GetCartUseCase
        // ==========================================
        [Fact]
        public async Task GetCart_ExistingCart_ReturnsCartDto()
        {
            // Arrange
            var userId = "user-123";
            var cart = new SalesCart(userId);
            cart.AddItem("p1", "SKU-1", "Prod 1", "Var 1", 100m, "img1");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(cart));

            var useCase = new GetCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(100m, result.SubTotal);
            Assert.Single(result.Items);
            var item = result.Items.First();
            Assert.Equal("p1", item.ProductId);
            Assert.Equal("SKU-1", item.VariantId);
            Assert.Equal("Prod 1", item.ProductName);
            Assert.Equal("Var 1", item.VariantName);
            Assert.Equal(100m, item.Price);
            Assert.Equal(1, item.Quantity);
            Assert.Equal("img1", item.Image);
        }

        [Fact]
        public async Task GetCart_NewCart_CreatesAndSavesAndReturnsEmpty()
        {
            // Arrange
            var userId = "user-456";
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(null));

            var useCase = new GetCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalItems);
            await _salesRepository.Received(1).SaveCartAsync(Arg.Is<SalesCart>(c => c.UserId == userId));
        }

        // ==========================================
        // GetMiniCartUseCase
        // ==========================================
        [Fact]
        public async Task GetMiniCart_ExistingCart_ReturnsTotalItems()
        {
            // Arrange
            var userId = "user-123";
            var cart = new SalesCart(userId);
            cart.AddItem("p1", "SKU-1", "Prod 1", "Var 1", 100m, "img1");
            cart.AddItem("p2", "SKU-2", "Prod 2", "Var 2", 50m, "img2");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(cart));

            var useCase = new GetMiniCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId);

            // Assert
            Assert.Equal(2, result.Quantity);
        }

        [Fact]
        public async Task GetMiniCart_MissingCart_ReturnsZero()
        {
            // Arrange
            var userId = "user-456";
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(null));

            var useCase = new GetMiniCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId);

            // Assert
            Assert.Equal(0, result.Quantity);
        }

        // ==========================================
        // GetOrderByIdUseCase
        // ==========================================
        [Fact]
        public async Task GetOrderById_ReturnsReceipt()
        {
            // Arrange
            var orderId = "ORD-111";
            var receipt = new OrderReceiptDto(orderId, "First", "Last", "store-1", new List<string>(), new List<CartItemDto>(), 100, 19, 119, DateTime.UtcNow);
            _salesRepository.GetOrderByIdAsync(orderId).Returns(Task.FromResult<OrderReceiptDto?>(receipt));

            var useCase = new GetOrderByIdUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(orderId);

            // Assert
            Assert.Same(receipt, result);
        }

        // ==========================================
        // RemoveFromCartUseCase
        // ==========================================
        [Fact]
        public async Task RemoveFromCart_ExistingCart_RemovesItemAndSaves()
        {
            // Arrange
            var userId = "user-123";
            var cart = new SalesCart(userId);
            cart.AddItem("p1", "SKU-1", "Prod 1", "Var 1", 100m, "img1");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(cart));

            var useCase = new RemoveFromCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId, "SKU-1");

            // Assert
            Assert.Empty(result.Items);
            await _salesRepository.Received(1).SaveCartAsync(cart);
        }

        [Fact]
        public async Task RemoveFromCart_MissingCart_ReturnsEmptyCart()
        {
            // Arrange
            var userId = "user-456";
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(null));

            var useCase = new RemoveFromCartUseCase(_salesRepository);

            // Act
            var result = await useCase.ExecuteAsync(userId, "SKU-1");

            // Assert
            Assert.Empty(result.Items);
            await _salesRepository.DidNotReceive().SaveCartAsync(Arg.Any<SalesCart>());
        }

        // ==========================================
        // AddToCartUseCase
        // ==========================================
        [Fact]
        public async Task AddToCart_ProductNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var command = new AddToCartCommand("user-123", "SKU-MISSING");
            _catalogService.GetProductBySkuAsync("SKU-MISSING").Returns(Task.FromResult<CatalogProductInfo?>(null));

            var useCase = new AddToCartUseCase(_salesRepository, _catalogService);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => useCase.ExecuteAsync(command));
        }

        [Fact]
        public async Task AddToCart_ValidProduct_AddsToCartAndSaves()
        {
            // Arrange
            var command = new AddToCartCommand("user-123", "SKU-1");
            var prodInfo = new CatalogProductInfo("p1", "SKU-1", "Product 1", "Variant 1", 150m, "img1");
            _catalogService.GetProductBySkuAsync("SKU-1").Returns(Task.FromResult<CatalogProductInfo?>(prodInfo));

            var cart = new SalesCart("user-123");
            _salesRepository.GetCartByUserIdAsync("user-123").Returns(Task.FromResult<SalesCart?>(cart));

            var useCase = new AddToCartUseCase(_salesRepository, _catalogService);

            // Act
            var result = await useCase.ExecuteAsync(command);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalItems);
            Assert.Equal(150m, result.SubTotal);
            await _salesRepository.Received(1).SaveCartAsync(cart);
        }

        [Fact]
        public async Task AddToCart_MissingCart_CreatesCartAddsAndSaves()
        {
            // Arrange
            var command = new AddToCartCommand("user-123", "SKU-1");
            var prodInfo = new CatalogProductInfo("p1", "SKU-1", "Product 1", "Variant 1", 150m, "img1");
            _catalogService.GetProductBySkuAsync("SKU-1").Returns(Task.FromResult<CatalogProductInfo?>(prodInfo));

            _salesRepository.GetCartByUserIdAsync("user-123").Returns(Task.FromResult<SalesCart?>(null));

            var useCase = new AddToCartUseCase(_salesRepository, _catalogService);

            // Act
            var result = await useCase.ExecuteAsync(command);

            // Assert
            Assert.Single(result.Items);
            await _salesRepository.Received(1).SaveCartAsync(Arg.Is<SalesCart>(c => c.UserId == "user-123"));
        }

        // ==========================================
        // CheckoutUseCase
        // ==========================================
        [Fact]
        public async Task Checkout_CartNullOrEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = "user-123";
            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string>());
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(null));

            var useCase = new CheckoutUseCase(_salesRepository, _inventoryService, _eventBus);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(userId, payload));
        }

        [Fact]
        public async Task Checkout_InsufficientStock_ThrowsInvalidOperationExceptionAndAborts()
        {
            // Arrange
            var userId = "user-123";
            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string>());
            var cart = new SalesCart(userId);
            cart.AddItem("p1", "SKU-1", "Prod 1", "Var 1", 100m, "img1");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(cart));

            var transaction = Substitute.For<IDbTransactionWrapper>();
            _salesRepository.BeginTransactionAsync().Returns(Task.FromResult(transaction));

            _inventoryService.DecreaseStockAsync("SKU-1", 1).Returns(Task.FromResult(false)); // No stock!

            var useCase = new CheckoutUseCase(_salesRepository, _inventoryService, _eventBus);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(userId, payload));
            Assert.Contains("Stock insuficiente", ex.Message);

            await transaction.Received(1).RollbackAsync();
            await _salesRepository.DidNotReceive().SaveOrderReceiptAsync(Arg.Any<OrderReceiptDto>());
        }

        [Fact]
        public async Task Checkout_Success_CommitsAndPublishesEvent()
        {
            // Arrange
            var userId = "user-123";
            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string> { "Extra-1" });
            var cart = new SalesCart(userId);
            cart.AddItem("p1", "SKU-1", "Prod 1", "Var 1", 100m, "img1");
            _salesRepository.GetCartByUserIdAsync(userId).Returns(Task.FromResult<SalesCart?>(cart));

            var transaction = Substitute.For<IDbTransactionWrapper>();
            _salesRepository.BeginTransactionAsync().Returns(Task.FromResult(transaction));

            _inventoryService.DecreaseStockAsync("SKU-1", 1).Returns(Task.FromResult(true));

            var useCase = new CheckoutUseCase(_salesRepository, _inventoryService, _eventBus);

            // Act
            var result = await useCase.ExecuteAsync(userId, payload);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("ORD-", result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("store-1", result.StoreId);
            Assert.Contains("Extra-1", result.ExtraPickups);
            Assert.Single(result.Items);
            Assert.Equal(100m, result.SubTotal);
            Assert.Equal(121m, result.Total);

            await _salesRepository.Received(1).SaveOrderReceiptAsync(result);
            Assert.Empty(cart.Items); // Cart should be cleared
            await _salesRepository.Received(1).SaveCartAsync(cart);
            await transaction.Received(1).CommitAsync();
            await _eventBus.Received(1).PublishAsync(
                topic: "sales.orders.placed",
                key: result.Id,
                message: Arg.Is<SalesOrderPlacedEvent>(e => e.OrderId == result.Id && e.Items.Count() == 1)
            );
        }
    }
}
