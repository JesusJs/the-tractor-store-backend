using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using TractorEcommerce.Modules.Sales.Application.Interfaces;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Application.UseCase;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using TractorEcommerce.Modules.Shared.Application.Events;
using Xunit;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Tests.Controllers
{
    public class SalesControllerTests
    {
        private readonly ISalesRepository _salesRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IEventBus _eventBus;
        private readonly ICatalogService _catalogService;
        private readonly SalesController _controller;
        private readonly DefaultHttpContext _httpContext;

        public SalesControllerTests()
        {
            _salesRepository = Substitute.For<ISalesRepository>();
            _inventoryService = Substitute.For<IInventoryService>();
            _eventBus = Substitute.For<IEventBus>();
            _catalogService = Substitute.For<ICatalogService>();

            var checkoutUseCase = new CheckoutUseCase(_salesRepository, _inventoryService, _eventBus);
            var addToCartUseCase = new AddToCartUseCase(_salesRepository, _catalogService);
            var removeFromCartUseCase = new RemoveFromCartUseCase(_salesRepository);
            var getCartUseCase = new GetCartUseCase(_salesRepository);
            var getMiniCartUseCase = new GetMiniCartUseCase(_salesRepository);
            var getOrderByIdUseCase = new GetOrderByIdUseCase(_salesRepository);

            _controller = new SalesController(
                checkoutUseCase,
                addToCartUseCase,
                removeFromCartUseCase,
                getCartUseCase,
                getMiniCartUseCase,
                getOrderByIdUseCase
            );

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public async Task GetCart_ShouldReturnCartDto()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var cart = new Cart(cartId);
            _salesRepository.GetCartByUserIdAsync(cartId).Returns(cart);

            // Act
            var result = await _controller.GetCart();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cartDto = Assert.IsType<CartDto>(okResult.Value);
            Assert.Equal(0, cartDto.TotalItems);
        }

        [Fact]
        public async Task GetMiniCart_ShouldReturnMiniCartDto()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var cart = new Cart(cartId);
            cart.AddItem("p-1", "SKU-1", "Prod 1", "Var 1", 100, "img");
            _salesRepository.GetCartByUserIdAsync(cartId).Returns(cart);

            // Act
            var result = await _controller.GetMiniCart();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var miniCartDto = Assert.IsType<MiniCartDto>(okResult.Value);
            Assert.Equal(1, miniCartDto.Quantity);
        }

        [Fact]
        public async Task AddToCart_WithValidSku_ShouldAddAndReturnCart()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var sku = "SKU-VALID";
            var productInfo = new CatalogProductInfo("p-1", sku, "Tractor 1", "Default", 12000, "image-url");
            _catalogService.GetProductBySkuAsync(sku).Returns(productInfo);

            var cart = new Cart(cartId);
            _salesRepository.GetCartByUserIdAsync(cartId).Returns(cart);

            // Act
            var request = new AddToCartRequest(sku);
            var result = await _controller.AddToCart(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cartDto = Assert.IsType<CartDto>(okResult.Value);
            Assert.Equal(1, cartDto.TotalItems);
            Assert.Equal(12000, cartDto.SubTotal);
            Assert.Equal(2520, cartDto.Tax);
            Assert.Equal(14520, cartDto.Total);
        }

        [Fact]
        public async Task AddToCart_WithInvalidSku_ShouldReturnNotFound()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var sku = "SKU-INVALID";
            _catalogService.GetProductBySkuAsync(sku).Returns((CatalogProductInfo?)null);

            // Act
            var request = new AddToCartRequest(sku);
            var result = await _controller.AddToCart(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task RemoveFromCart_ShouldRemoveItemAndReturnCart()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var sku = "SKU-1";
            var cart = new Cart(cartId);
            cart.AddItem("p-1", sku, "Prod 1", "Var 1", 100, "img");
            _salesRepository.GetCartByUserIdAsync(cartId).Returns(cart);

            // Act
            var result = await _controller.RemoveFromCart(sku);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var cartDto = Assert.IsType<CartDto>(okResult.Value);
            Assert.Equal(0, cartDto.TotalItems);
        }

        [Fact]
        public async Task PlaceOrder_WithValidPayload_ShouldReturnReceipt()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            var cart = new Cart(cartId);
            cart.AddItem("p-1", "SKU-1", "Prod 1", "Var 1", 100, "img");
            _salesRepository.GetCartByUserIdAsync(cartId).Returns(cart);

            var transaction = Substitute.For<IDbTransactionWrapper>();
            _salesRepository.BeginTransactionAsync().Returns(transaction);
            _inventoryService.DecreaseStockAsync("SKU-1", 1).Returns(true);

            // Act
            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string>());
            var result = await _controller.PlaceOrder(payload);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var receipt = Assert.IsType<OrderReceiptDto>(okResult.Value);
            Assert.Equal("John", receipt.FirstName);
            Assert.Equal("store-1", receipt.StoreId);
        }

        [Fact]
        public async Task PlaceOrder_WithEmptyCart_ShouldReturnBadRequest()
        {
            // Arrange
            var cartId = "test-user-id";
            _httpContext.Request.Headers.Cookie = "tractor_session=" + cartId;

            _salesRepository.GetCartByUserIdAsync(cartId).Returns((Cart?)null);

            // Act
            var payload = new OrderPayloadDto("John", "Doe", "store-1", new List<string>());
            var result = await _controller.PlaceOrder(payload);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetOrder_WithExistingId_ShouldReturnReceipt()
        {
            // Arrange
            var orderId = "ORD-123456";
            var receipt = new OrderReceiptDto(orderId, "John", "Doe", "store-1", new List<string>(), new List<CartItemDto>(), 100, 21, 121, DateTime.UtcNow);
            _salesRepository.GetOrderByIdAsync(orderId).Returns(receipt);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedReceipt = Assert.IsType<OrderReceiptDto>(okResult.Value);
            Assert.Equal(orderId, returnedReceipt.Id);
        }

        [Fact]
        public async Task GetOrder_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var orderId = "ORD-MISSING";
            _salesRepository.GetOrderByIdAsync(orderId).Returns((OrderReceiptDto?)null);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
