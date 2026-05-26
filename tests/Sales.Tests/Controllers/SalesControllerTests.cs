using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using TractorEcommerce.Api.Controllers;
using CartUseCase = TractorEcommerce.Modules.Cart.Application.UseCase;
using OrderUseCase = TractorEcommerce.Modules.Order.Application.UseCase;
using TractorEcommerce.Modules.Cart.Application.Commands;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Sales.Tests.Controllers
{
    // =========================================================================
    // CartController Tests
    // =========================================================================
    public class CartControllerTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly CartUseCase.AddToCartUseCase _addToCartUseCase;
        private readonly CartUseCase.RemoveFromCartUseCase _removeFromCartUseCase;
        private readonly CartUseCase.GetCartUseCase _getCartUseCase;
        private readonly ILogger<CartController> _logger;
        private readonly CartController _controller;
        private readonly DefaultHttpContext _httpContext;

        public CartControllerTests()
        {
            _cartRepository = Substitute.For<ICartRepository>();
            _addToCartUseCase = new CartUseCase.AddToCartUseCase(_cartRepository);
            _removeFromCartUseCase = new CartUseCase.RemoveFromCartUseCase(_cartRepository);
            _getCartUseCase = new CartUseCase.GetCartUseCase(_cartRepository);
            _logger = Substitute.For<ILogger<CartController>>();

            _controller = new CartController(
                _addToCartUseCase,
                _removeFromCartUseCase,
                _getCartUseCase,
                _logger
            );

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public async Task GetCart_WithAnonymousCookie_ReturnsCart()
        {
            var sessionCartId = "session-12345";
            _httpContext.Request.Headers.Cookie = $"tractor_session={sessionCartId}";

            var cart = new TractorEcommerce.Modules.Cart.Domain.Entities.Cart(sessionCartId);
            _cartRepository.GetByUserIdAsync(sessionCartId).Returns(cart);

            var result = await _controller.GetCart();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCart = Assert.IsType<TractorEcommerce.Modules.Cart.Domain.Entities.Cart>(okResult.Value);
            Assert.Equal(sessionCartId, returnedCart.UserId);
        }

        [Fact]
        public async Task GetCart_WithAuthenticatedUser_ReturnsCart()
        {
            var userId = "user-auth-123";
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _httpContext.User = new ClaimsPrincipal(identity);

            var cart = new TractorEcommerce.Modules.Cart.Domain.Entities.Cart(userId);
            _cartRepository.GetByUserIdAsync(userId).Returns(cart);

            var result = await _controller.GetCart();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCart = Assert.IsType<TractorEcommerce.Modules.Cart.Domain.Entities.Cart>(okResult.Value);
            Assert.Equal(userId, returnedCart.UserId);
        }

        [Fact]
        public async Task AddToCart_EmptySku_ThrowsArgumentException()
        {
            var request = new AddToCartRequest("", 100);
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.AddToCart(request));
        }

        [Fact]
        public async Task AddToCart_ValidRequest_ReturnsCart()
        {
            var cartId = "session-123";
            _httpContext.Request.Headers.Cookie = $"tractor_session={cartId}";

            var request = new AddToCartRequest("SKU-1", 100);
            var cart = new TractorEcommerce.Modules.Cart.Domain.Entities.Cart(cartId);
            _cartRepository.GetByUserIdAsync(cartId).Returns(cart);

            var result = await _controller.AddToCart(request);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCart = Assert.IsType<TractorEcommerce.Modules.Cart.Domain.Entities.Cart>(okResult.Value);
            Assert.Equal(cartId, returnedCart.UserId);
            await _cartRepository.Received(1).SaveAsync(cart);
        }

        [Fact]
        public async Task RemoveFromCart_EmptySku_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.RemoveFromCart(""));
        }

        [Fact]
        public async Task RemoveFromCart_ValidRequest_ReturnsCart()
        {
            var cartId = "session-123";
            _httpContext.Request.Headers.Cookie = $"tractor_session={cartId}";

            var cart = new TractorEcommerce.Modules.Cart.Domain.Entities.Cart(cartId);
            _cartRepository.GetByUserIdAsync(cartId).Returns(cart);

            var result = await _controller.RemoveFromCart("SKU-1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCart = Assert.IsType<TractorEcommerce.Modules.Cart.Domain.Entities.Cart>(okResult.Value);
            Assert.Equal(cartId, returnedCart.UserId);
            await _cartRepository.Received(1).SaveAsync(cart);
        }
    }

    // =========================================================================
    // OrderController Tests
    // =========================================================================
    public class OrderControllerTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderUseCase.CheckoutUseCase> _checkoutUseCaseLogger;
        private readonly OrderUseCase.CheckoutUseCase _checkoutUseCase;
        private readonly ILogger<OrderController> _logger;
        private readonly OrderController _controller;
        private readonly DefaultHttpContext _httpContext;

        public OrderControllerTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _eventBus = Substitute.For<IEventBus>();
            _checkoutUseCaseLogger = Substitute.For<ILogger<OrderUseCase.CheckoutUseCase>>();
            _checkoutUseCase = new OrderUseCase.CheckoutUseCase(_orderRepository, _eventBus, _checkoutUseCaseLogger);
            _logger = Substitute.For<ILogger<OrderController>>();

            _controller = new OrderController(
                _checkoutUseCase,
                _logger
            );

            _httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        [Fact]
        public async Task PlaceOrder_NullPayload_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.PlaceOrder(null!));
        }

        [Fact]
        public async Task PlaceOrder_EmptyStoreId_ThrowsArgumentException()
        {
            var payload = new OrderPayloadDto("John", "Doe", "", null, new List<OrderPayloadItemDto>());
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.PlaceOrder(payload));
        }

        [Fact]
        public async Task PlaceOrder_NoCookieSession_ThrowsInvalidOperationException()
        {
            var payload = new OrderPayloadDto("John", "Doe", "store-1", null, new List<OrderPayloadItemDto>());
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PlaceOrder(payload));
        }

        [Fact]
        public async Task PlaceOrder_ValidRequest_ReturnsReceipt()
        {
            var cartId = "session-123";
            _httpContext.Request.Headers.Cookie = $"tractor_session={cartId}";

            var payload = new OrderPayloadDto("John", "Doe", "store-1", null, new List<OrderPayloadItemDto>
            {
                new OrderPayloadItemDto("prod-1", "SKU-1", "Tractor", "STD", 100, 1, "img")
            });

            var result = await _controller.PlaceOrder(payload);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var receipt = Assert.IsType<OrderReceiptDto>(okResult.Value);
            Assert.StartsWith("ORD-", receipt.Id);
            Assert.Equal("John", receipt.FirstName);
            Assert.Equal("Doe", receipt.LastName);
            Assert.Equal("store-1", receipt.StoreId);
        }
    }
}
