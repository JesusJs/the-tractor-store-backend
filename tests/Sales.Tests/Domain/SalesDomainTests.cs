using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using CartEntity = TractorEcommerce.Modules.Cart.Domain.Entities.Cart;
using TractorEcommerce.Modules.Cart.Domain.Entities;
using TractorEcommerce.Modules.Order.Domain.Entities;

namespace TractorEcommerce.Modules.Sales.Tests.Domain
{
    // =========================================================================
    // Cart & CartItem Domain Tests
    // =========================================================================
    public class CartDomainTests
    {
        [Fact]
        public void Cart_Constructor_SetsUserId()
        {
            var cart = new CartEntity("user-1");
            Assert.Equal("user-1", cart.UserId);
            Assert.Empty(cart.Items);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Cart_Constructor_EmptyUserId_ThrowsArgumentException(string? userId)
        {
            Assert.Throws<ArgumentException>(() => new CartEntity(userId!));
        }

        [Fact]
        public void Cart_AddItem_AddsNewItem()
        {
            var cart = new CartEntity("user-1");
            cart.AddItem("SKU-1", 2, 100);

            Assert.Single(cart.Items);
            var item = cart.Items.First();
            Assert.Equal("SKU-1", item.Sku);
            Assert.Equal(2, item.Quantity);
            Assert.Equal(100, item.Price);
            Assert.Equal(200, item.Total);
        }

        [Fact]
        public void Cart_AddItem_ExistingSku_IncrementsQuantity()
        {
            var cart = new CartEntity("user-1");
            cart.AddItem("SKU-1", 2, 100);
            cart.AddItem("SKU-1", 3, 100);

            Assert.Single(cart.Items);
            var item = cart.Items.First();
            Assert.Equal("SKU-1", item.Sku);
            Assert.Equal(5, item.Quantity);
        }

        [Fact]
        public void Cart_AddItem_QuantityZeroOrLess_ThrowsArgumentException()
        {
            var cart = new CartEntity("user-1");
            Assert.Throws<ArgumentException>(() => cart.AddItem("SKU-1", 0, 100));
            Assert.Throws<ArgumentException>(() => cart.AddItem("SKU-1", -1, 100));
        }

        [Fact]
        public void Cart_Clear_RemovesAllItems()
        {
            var cart = new CartEntity("user-1");
            cart.AddItem("SKU-1", 2, 100);
            cart.Clear();

            Assert.Empty(cart.Items);
            Assert.Equal(0, cart.TotalItems);
            Assert.Equal(0, cart.SubTotal);
        }

        [Fact]
        public void Cart_Calculates_SubTotal_Tax_And_Total_Correctly()
        {
            var cart = new CartEntity("user-1");
            cart.AddItem("SKU-1", 2, 100); // SubTotal = 200
            cart.AddItem("SKU-2", 1, 50);  // SubTotal = 250

            Assert.Equal(3, cart.TotalItems);
            Assert.Equal(250m, cart.SubTotal);
            Assert.Equal(250m * 0.19m, cart.Tax); // 19% Tax
            Assert.Equal(250m * 1.19m, cart.Total);
        }
    }

    // =========================================================================
    // ShoppingCart Domain Tests
    // =========================================================================
    public class ShoppingCartTests
    {
        [Fact]
        public void ShoppingCart_Constructor_SetsId()
        {
            var cart = new ShoppingCart("session-1");
            Assert.Equal("session-1", cart.Id);
            Assert.Empty(cart.Items);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ShoppingCart_Constructor_EmptyId_ThrowsArgumentException(string? id)
        {
            Assert.Throws<ArgumentException>(() => new ShoppingCart(id!));
        }

        [Fact]
        public void ShoppingCart_AddItem_AddsNewItem()
        {
            var cart = new ShoppingCart("session-1");
            cart.AddItem(new CartItem("SKU-1", 2, 100));

            Assert.Single(cart.Items);
            Assert.Equal(200m, cart.TotalAmount);
        }

        [Fact]
        public void ShoppingCart_AddItem_ExistingSku_IncrementsQuantity()
        {
            var cart = new ShoppingCart("session-1");
            cart.AddItem(new CartItem("SKU-1", 2, 100));
            cart.AddItem(new CartItem("SKU-1", 3, 100));

            Assert.Single(cart.Items);
            Assert.Equal(500m, cart.TotalAmount);
            Assert.Equal(5, cart.Items.First().Quantity);
        }

        [Fact]
        public void ShoppingCart_RemoveItem_RemovesSku()
        {
            var cart = new ShoppingCart("session-1");
            cart.AddItem(new CartItem("SKU-1", 2, 100));
            cart.RemoveItem("SKU-1");

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void ShoppingCart_Clear_RemovesAllItems()
        {
            var cart = new ShoppingCart("session-1");
            cart.AddItem(new CartItem("SKU-1", 2, 100));
            cart.Clear();

            Assert.Empty(cart.Items);
        }
    }

    // =========================================================================
    // CustomerOrder & OrderLineItem Domain Tests
    // =========================================================================
    public class CustomerOrderTests
    {
        [Fact]
        public void CustomerOrder_Constructor_SetsProperties()
        {
            var orderId = Guid.NewGuid();
            var lineItems = new List<OrderLineItem>
            {
                new OrderLineItem(Guid.NewGuid(), "SKU-1", 2, 100) { ProductName = "Tractor A" }
            };

            var order = new CustomerOrder(orderId, "customer-123", lineItems);

            Assert.Equal(orderId, order.Id);
            Assert.Equal("customer-123", order.CustomerId);
            Assert.Single(order.Items);
            Assert.Equal(200m, order.TotalAmount);
            Assert.Equal("Pending", order.Status);
            Assert.True(order.CreatedAt <= DateTime.UtcNow);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void CustomerOrder_EmptyCustomerId_ThrowsArgumentException(string? customerId)
        {
            var lineItems = new List<OrderLineItem>
            {
                new OrderLineItem(Guid.NewGuid(), "SKU-1", 2, 100)
            };
            Assert.Throws<ArgumentException>(() => new CustomerOrder(Guid.NewGuid(), customerId!, lineItems));
        }

        [Fact]
        public void CustomerOrder_NullOrEmptyItems_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new CustomerOrder(Guid.NewGuid(), "customer-123", null!));
            Assert.Throws<ArgumentException>(() => new CustomerOrder(Guid.NewGuid(), "customer-123", new List<OrderLineItem>()));
        }

        [Fact]
        public void CustomerOrder_Complete_UpdatesStatus()
        {
            var lineItems = new List<OrderLineItem>
            {
                new OrderLineItem(Guid.NewGuid(), "SKU-1", 2, 100)
            };
            var order = new CustomerOrder(Guid.NewGuid(), "customer-123", lineItems);
            order.Complete();

            Assert.Equal("Completed", order.Status);
        }
    }
}
