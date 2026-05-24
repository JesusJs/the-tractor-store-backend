using System;
using System.Collections.Generic;
using System.Linq;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using Xunit;

namespace TractorEcommerce.Modules.Sales.Tests.Domain
{
    // ---------------------------------------------------------------------------
    // OrderReceipt Domain Tests
    // ---------------------------------------------------------------------------
    public class OrderReceiptTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            // Arrange
            var id = "ORD-001";
            var placed = DateTime.UtcNow;

            // Act
            var receipt = new OrderReceipt(id, "John", "Doe", "store-1", null, 10000, 2100, 12100, placed);

            // Assert
            Assert.Equal(id, receipt.Id);
            Assert.Equal("John", receipt.FirstName);
            Assert.Equal("Doe", receipt.LastName);
            Assert.Equal("store-1", receipt.StoreId);
            Assert.Null(receipt.ExtraPickups);
            Assert.Equal(10000, receipt.SubTotal);
            Assert.Equal(2100, receipt.Tax);
            Assert.Equal(12100, receipt.Total);
            Assert.Equal(placed, receipt.PlacedAt);
        }

        [Fact]
        public void Constructor_WithExtraPickups_SetsExtraPickups()
        {
            // Act
            var receipt = new OrderReceipt("ORD-002", "Ana", "Ruiz", "store-2", "extra-store-A", 5000, 1050, 6050, DateTime.UtcNow);

            // Assert
            Assert.Equal("extra-store-A", receipt.ExtraPickups);
        }

        [Fact]
        public void Items_InitiallyEmpty()
        {
            // Act
            var receipt = new OrderReceipt("ORD-003", "Luis", "García", "store-3", null, 0, 0, 0, DateTime.UtcNow);

            // Assert
            Assert.Empty(receipt.Items);
        }

        [Fact]
        public void AddItem_AddsToItemsCollection()
        {
            // Arrange
            var receipt = new OrderReceipt("ORD-004", "Maria", "Lopez", "store-4", null, 5000, 1050, 6050, DateTime.UtcNow);
            var item = new OrderReceiptItem("SKU-A", "p-1", "Tractor A", "Standard", 5000, 1, "img-a");

            // Act
            receipt.AddItem(item);

            // Assert
            Assert.Single(receipt.Items);
        }

        [Fact]
        public void AddItem_MultipleItems_AllAppear()
        {
            // Arrange
            var receipt = new OrderReceipt("ORD-005", "Pedro", "Sanz", "store-5", null, 20000, 4200, 24200, DateTime.UtcNow);

            // Act
            receipt.AddItem(new OrderReceiptItem("SKU-A", "p-1", "Tractor A", "STD", 10000, 1, "img-a"));
            receipt.AddItem(new OrderReceiptItem("SKU-B", "p-2", "Tractor B", "GPS", 10000, 1, "img-b"));

            // Assert
            Assert.Equal(2, receipt.Items.Count);
        }
    }

    // ---------------------------------------------------------------------------
    // OrderReceiptItem Domain Tests
    // ---------------------------------------------------------------------------
    public class OrderReceiptItemTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            // Act
            var item = new OrderReceiptItem("SKU-001", "prod-1", "Tractor Pro", "GPS Edition", 15000, 2, "img-url");

            // Assert
            Assert.Equal("SKU-001", item.Sku);
            Assert.Equal("prod-1", item.ProductId);
            Assert.Equal("Tractor Pro", item.ProductName);
            Assert.Equal("GPS Edition", item.VariantName);
            Assert.Equal(15000, item.Price);
            Assert.Equal(2, item.Quantity);
            Assert.Equal("img-url", item.Image);
        }

        [Fact]
        public void Constructor_WithQuantityOne_SetsQuantityOne()
        {
            // Act
            var item = new OrderReceiptItem("SKU-002", "prod-2", "Mini Tractor", "Basic", 3000, 1, "img-mini");

            // Assert
            Assert.Equal(1, item.Quantity);
        }

        [Fact]
        public void Constructor_WithZeroPrice_AllowsIt()
        {
            // Act
            var item = new OrderReceiptItem("SKU-FREE", "prod-free", "Free Item", "Promo", 0, 1, "img-free");

            // Assert
            Assert.Equal(0, item.Price);
        }
    }

    // ---------------------------------------------------------------------------
    // CartItem Domain Tests (boosting 95.2% → 100%)
    // ---------------------------------------------------------------------------
    public class CartItemDomainTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            // Act
            var item = new CartItem("prod-1", "SKU-001", "Tractor Pro", "GPS", 15000, "img-url");

            // Assert
            Assert.Equal("prod-1", item.ProductId);
            Assert.Equal("SKU-001", item.VariantId);
            Assert.Equal("Tractor Pro", item.ProductName);
            Assert.Equal("GPS", item.VariantName);
            Assert.Equal(15000, item.Price);
            Assert.Equal(1, item.Quantity); // default
            Assert.Equal("img-url", item.Image);
        }

        [Fact]
        public void Constructor_WithExplicitQuantity_SetsQuantity()
        {
            // Act
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "STD", 5000, "img", quantity: 3);

            // Assert
            Assert.Equal(3, item.Quantity);
        }

        [Fact]
        public void IncrementQuantity_IncreasesByOne()
        {
            // Arrange
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "STD", 5000, "img");

            // Act
            item.IncrementQuantity();

            // Assert
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void IncrementQuantity_MultipleTimes_AccumulatesCorrectly()
        {
            // Arrange
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "STD", 5000, "img");

            // Act
            item.IncrementQuantity();
            item.IncrementQuantity();
            item.IncrementQuantity();

            // Assert
            Assert.Equal(4, item.Quantity);
        }
    }

    // ---------------------------------------------------------------------------
    // OrderPlacedEvent & OrderEventItem Domain Tests (50% → 100%)
    // ---------------------------------------------------------------------------
    public class OrderPlacedEventTests
    {
        [Fact]
        public void OrderPlacedEvent_SetsAllProperties()
        {
            // Arrange
            var items = new List<Sales.Domain.Events.OrderEventItem>
            {
                new("SKU-A", 2),
                new("SKU-B", 1)
            };
            var now = DateTime.UtcNow;

            // Act
            var evt = new Sales.Domain.Events.OrderPlacedEvent("ORD-001", items, now);

            // Assert
            Assert.Equal("ORD-001", evt.OrderId);
            Assert.Equal(2, evt.Items.Count());
            Assert.Equal(now, evt.OccurredAt);
        }

        [Fact]
        public void OrderEventItem_SetsSkuAndQuantity()
        {
            // Act
            var item = new Sales.Domain.Events.OrderEventItem("SKU-TEST", 5);

            // Assert
            Assert.Equal("SKU-TEST", item.Sku);
            Assert.Equal(5, item.Quantity);
        }

        [Fact]
        public void OrderPlacedEvent_WithEmptyItems_AllowsEmptyCollection()
        {
            // Act
            var evt = new Sales.Domain.Events.OrderPlacedEvent(
                "ORD-EMPTY",
                Enumerable.Empty<Sales.Domain.Events.OrderEventItem>(),
                DateTime.UtcNow);

            // Assert
            Assert.Empty(evt.Items);
        }
    }

    // ---------------------------------------------------------------------------
    // Cart Domain Tests (boosting 96.8% → 100%)
    // ---------------------------------------------------------------------------
    public class CartDomainTests
    {
        [Fact]
        public void Constructor_NullUserId_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Cart(null!));
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            // Arrange
            var cart = new Cart("user-1");
            cart.AddItem("p-1", "SKU-A", "Tractor A", "STD", 5000, "img");
            cart.AddItem("p-2", "SKU-B", "Tractor B", "GPS", 7000, "img");

            // Act
            cart.Clear();

            // Assert
            Assert.Equal(0, cart.TotalItems);
            Assert.Empty(cart.Items);
        }

        [Fact]
        public void RemoveItem_NonExistentSku_DoesNotThrow()
        {
            // Arrange
            var cart = new Cart("user-1");

            // Act & Assert (should not throw)
            cart.RemoveItem("SKU-MISSING");
            Assert.Equal(0, cart.TotalItems);
        }

        [Fact]
        public void Tax_IsCalculatedAt21Percent()
        {
            // Arrange
            var cart = new Cart("user-tax");
            cart.AddItem("p-1", "SKU-A", "Tractor A", "STD", 1000, "img");

            // Assert: 1000 * 0.21 = 210
            Assert.Equal(210, cart.Tax);
            Assert.Equal(1210, cart.Total);
        }

        [Fact]
        public void AddItem_DuplicateSku_IncrementsQuantityNotDuplicatesEntry()
        {
            // Arrange
            var cart = new Cart("user-dup");
            cart.AddItem("p-1", "SKU-SAME", "Tractor A", "STD", 5000, "img");

            // Act — add the same SKU again
            cart.AddItem("p-1", "SKU-SAME", "Tractor A", "STD", 5000, "img");

            // Assert — still 1 line item, but quantity = 2
            Assert.Single(cart.Items);
            Assert.Equal(2, cart.TotalItems);
            Assert.Equal(10000, cart.SubTotal);
        }

        [Fact]
        public void SubTotal_CalculatesCorrectlyWithMultipleItems()
        {
            // Arrange
            var cart = new Cart("user-sub");
            cart.AddItem("p-1", "SKU-A", "Tractor A", "STD", 5000, "img");
            cart.AddItem("p-2", "SKU-B", "Tractor B", "GPS", 3000, "img");

            // Assert
            Assert.Equal(8000, cart.SubTotal);
            Assert.Equal(2, cart.TotalItems);
        }
    }
}

