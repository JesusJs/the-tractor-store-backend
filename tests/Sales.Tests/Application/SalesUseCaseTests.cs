using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using TractorEcommerce.Modules.Cart.Application.Commands;
using TractorEcommerce.Modules.Cart.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Cart.Application.UseCase;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Application.UseCase;
using TractorEcommerce.Modules.Order.Application.UseCase.TractorEcommerce.Modules.Order.Application.UseCases;
using TractorEcommerce.Modules.Shared.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;
using CartEntity = TractorEcommerce.Modules.Cart.Domain.Entities.Cart;
using ShoppingCartEntity = TractorEcommerce.Modules.Cart.Domain.Entities.ShoppingCart;
using CartItemEntity = TractorEcommerce.Modules.Cart.Domain.Entities.CartItem;

namespace TractorEcommerce.Modules.Sales.Tests.Application
{
    // =========================================================================
    // Cart Use Case Tests
    // =========================================================================
    public class GetCartUseCaseTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly GetCartUseCase _useCase;

        public GetCartUseCaseTests()
        {
            _cartRepository = Substitute.For<ICartRepository>();
            _useCase = new GetCartUseCase(_cartRepository);
        }

        [Fact]
        public async Task Execute_WithExistingCart_ReturnsCart()
        {
            var userId = "user-1";
            var cart = new CartEntity(userId);
            cart.AddItem("SKU-1", 1, 100);
            _cartRepository.GetByUserIdAsync(userId).Returns(cart);

            var result = await _useCase.ExecuteAsync(userId);

            Assert.Same(cart, result);
            Assert.Equal(1, result.TotalItems);
        }

        [Fact]
        public async Task Execute_WithNoExistingCart_ReturnsNewEmptyCart()
        {
            var userId = "user-2";
            _cartRepository.GetByUserIdAsync(userId).Returns((CartEntity?)null);

            var result = await _useCase.ExecuteAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Empty(result.Items);
        }
    }

    public class AddToCartUseCaseTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly AddToCartUseCase _useCase;

        public AddToCartUseCaseTests()
        {
            _cartRepository = Substitute.For<ICartRepository>();
            _useCase = new AddToCartUseCase(_cartRepository);
        }

        [Fact]
        public async Task Execute_AddsItemAndSaves()
        {
            var userId = "user-1";
            var command = new AddToCartCommand(userId, "SKU-1", 2, 100);
            var cart = new CartEntity(userId);
            _cartRepository.GetByUserIdAsync(userId).Returns(cart);

            var result = await _useCase.ExecuteAsync(command);

            Assert.Single(result.Items);
            Assert.Equal(2, result.TotalItems);
            await _cartRepository.Received(1).SaveAsync(cart);
        }
    }

    public class RemoveFromCartUseCaseTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly RemoveFromCartUseCase _useCase;

        public RemoveFromCartUseCaseTests()
        {
            _cartRepository = Substitute.For<ICartRepository>();
            _useCase = new RemoveFromCartUseCase(_cartRepository);
        }

        [Fact]
        public async Task Execute_ClearsCartAndSaves()
        {
            var userId = "user-1";
            var cart = new CartEntity(userId);
            cart.AddItem("SKU-1", 2, 100);
            _cartRepository.GetByUserIdAsync(userId).Returns(cart);

            var result = await _useCase.ExecuteAsync(userId, "SKU-1");

            Assert.Empty(result.Items);
            await _cartRepository.Received(1).SaveAsync(cart);
        }
    }

    public class ClearCartUseCaseTests
    {
        private readonly ICartRepository _cartRepository;
        private readonly ClearCartUseCase _useCase;

        public ClearCartUseCaseTests()
        {
            _cartRepository = Substitute.For<ICartRepository>();
            _useCase = new ClearCartUseCase(_cartRepository);
        }

        [Fact]
        public async Task Execute_ClearsCartAndSaves()
        {
            var userId = "user-1";
            var cart = new CartEntity(userId);
            cart.AddItem("SKU-1", 2, 100);
            _cartRepository.GetByUserIdAsync(userId).Returns(cart);

            await _useCase.ExecuteAsync(userId);

            Assert.Empty(cart.Items);
            await _cartRepository.Received(1).SaveAsync(cart);
        }
    }

    public class CartCheckoutUseCaseTests
    {
        private readonly ICartSessionRepository _cartRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<TractorEcommerce.Modules.Cart.Application.UseCase.CheckoutUseCase> _logger;
        private readonly TractorEcommerce.Modules.Cart.Application.UseCase.CheckoutUseCase _useCase;

        public CartCheckoutUseCaseTests()
        {
            _cartRepository = Substitute.For<ICartSessionRepository>();
            _eventBus = Substitute.For<IEventBus>();
            _logger = Substitute.For<ILogger<TractorEcommerce.Modules.Cart.Application.UseCase.CheckoutUseCase>>();
            _useCase = new TractorEcommerce.Modules.Cart.Application.UseCase.CheckoutUseCase(_cartRepository, _eventBus, _logger);
        }

        [Fact]
        public async Task Execute_EmptyCart_ThrowsInvalidOperationException()
        {
            var cartId = "session-1";
            _cartRepository.GetCartAsync(cartId).Returns((ShoppingCartEntity)null!);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync(cartId, "customer-1"));
        }

        [Fact]
        public async Task Execute_WithItems_PublishesEvent()
        {
            var cartId = "session-1";
            var cart = new ShoppingCartEntity(cartId);
            cart.AddItem(new CartItemEntity("SKU-1", 2, 100));
            _cartRepository.GetCartAsync(cartId).Returns(cart);

            await _useCase.ExecuteAsync(cartId, "customer-1");

            await _eventBus.Received(1).PublishAsync(
                "checkout-requested-topic",
                cartId,
                Arg.Any<CheckoutRequestedEvent>()
            );
        }
    }

    // =========================================================================
    // Order Use Case Tests
    // =========================================================================
    public class OrderCheckoutUseCaseTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;
        private readonly TractorEcommerce.Modules.Order.Application.UseCase.CheckoutUseCase _useCase;

        public OrderCheckoutUseCaseTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _eventBus = Substitute.For<IEventBus>();
            _useCase = new TractorEcommerce.Modules.Order.Application.UseCase.CheckoutUseCase(_orderRepository, _eventBus);
        }

        [Fact]
        public async Task Execute_WithNoItems_ThrowsInvalidOperationException()
        {
            var payload = new OrderPayloadDto("John", "Doe", "store-1", null, new List<OrderPayloadItemDto>());
            await Assert.ThrowsAsync<InvalidOperationException>(() => _useCase.ExecuteAsync("user-1", payload));
        }

        [Fact]
        public async Task Execute_WithItems_SavesOrderAndPublishesEvent()
        {
            var payload = new OrderPayloadDto("John", "Doe", "store-1", "pickup-1", new List<OrderPayloadItemDto>
            {
                new OrderPayloadItemDto("prod-1", "SKU-1", "Tractor", "STD", 100, 2, "img")
            });

            var receipt = await _useCase.ExecuteAsync("user-1", payload);

            Assert.NotNull(receipt);
            Assert.StartsWith("ORD-", receipt.Id);
            Assert.Equal(200, receipt.SubTotal);
            Assert.Equal(38, receipt.Tax); // 200 * 0.19
            Assert.Equal(238, receipt.Total);

            await _orderRepository.Received(1).SaveOrderAsync(Arg.Is<OrderReceiptDto>(o => o.Id == receipt.Id));
            await _eventBus.Received(1).PublishAsync(
                "sales.orders.placed",
                receipt.Id,
                Arg.Any<OrderPlacedKafkaEvent>()
            );
        }
    }

    public class GetOrderByIdUseCaseTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly GetOrderByIdUseCase _useCase;

        public GetOrderByIdUseCaseTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _useCase = new GetOrderByIdUseCase(_orderRepository);
        }

        [Fact]
        public async Task Execute_ReturnsOrder()
        {
            var orderId = "ORD-123456";
            var receipt = new OrderReceiptDto(orderId, "John", "Doe", "store-1", null, new List<OrderItemDetailDto>(), 100, 19, 119, DateTime.UtcNow, "Pending");
            _orderRepository.GetOrderByIdAsync(orderId).Returns(receipt);

            var result = await _useCase.ExecuteAsync(orderId);

            Assert.Same(receipt, result);
        }
    }

    public class CreateOrderUseCaseTests
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;
        private readonly CreateOrderUseCase _useCase;

        public CreateOrderUseCaseTests()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _eventBus = Substitute.For<IEventBus>();
            _useCase = new CreateOrderUseCase(_orderRepository, _eventBus);
        }

        [Fact]
        public async Task Execute_CreatesOrderAndSavesAndPublishes()
        {
            var payload = new OrderPayloadDto("John", "Doe", "store-1", "pickup-1", new List<OrderPayloadItemDto>
            {
                new OrderPayloadItemDto("prod-1", "SKU-1", "Tractor", "STD", 100, 2, "img")
            });

            await _useCase.ExecuteAsync(payload);

            await _orderRepository.Received(1).SaveOrderAsync(Arg.Any<OrderReceiptDto>());
            await _eventBus.Received(1).PublishAsync(
                topic: "order.orders.placed",
                key: Arg.Any<string>(),
                message: Arg.Any<OrderPlacedIntegrationEvent>()
            );
        }
    }
}
