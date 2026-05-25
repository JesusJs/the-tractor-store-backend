using System;
using System.Reflection;
using System.Collections.Generic;
using TractorEcommerce.Modules.Inventory.Application.Events;
using TractorEcommerce.Modules.Inventory.Application.IntegrationEvents;
using TractorEcommerce.Modules.Inventory.Domain.Entities;

namespace TractorEcommerce.Modules.Inventory.Tests.Domain
{
    // =========================================================================
    // SkuStock Domain Tests
    // =========================================================================
    public class SkuStockDomainTests
    {
        [Fact]
        public void SkuStock_Constructor_SetsPropertiesCorrectly()
        {
            var stock = new SkuStock("tx-001", 50);

            Assert.Equal("TX-001", stock.Sku); // ToUpper()
            Assert.Equal(50, stock.AvailableStock);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void SkuStock_Constructor_EmptySku_ThrowsArgumentException(string sku)
        {
            Assert.Throws<ArgumentException>(() => new SkuStock(sku, 10));
        }

        [Fact]
        public void SkuStock_Constructor_NegativeStock_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new SkuStock("TX-001", -1));
        }

        [Fact]
        public void SkuStock_Constructor_ZeroStock_IsValid()
        {
            var stock = new SkuStock("TX-001", 0);
            Assert.Equal(0, stock.AvailableStock);
        }

        [Fact]
        public void SkuStock_SkuIsUppercased()
        {
            var stock = new SkuStock("tx-abc-001", 5);
            Assert.Equal("TX-ABC-001", stock.Sku);
        }

        // --- DeductStock ---

        [Fact]
        public void DeductStock_ValidQuantity_ReducesStock()
        {
            var stock = new SkuStock("TX-001", 20);
            stock.DeductStock(5);
            Assert.Equal(15, stock.AvailableStock);
        }

        [Fact]
        public void DeductStock_ExactStock_LeavesZero()
        {
            var stock = new SkuStock("TX-001", 10);
            stock.DeductStock(10);
            Assert.Equal(0, stock.AvailableStock);
        }

        [Fact]
        public void DeductStock_MoreThanAvailable_ThrowsInvalidOperationException()
        {
            var stock = new SkuStock("TX-001", 5);
            Assert.Throws<InvalidOperationException>(() => stock.DeductStock(10));
        }

        [Fact]
        public void DeductStock_ZeroQuantity_ThrowsArgumentException()
        {
            var stock = new SkuStock("TX-001", 10);
            Assert.Throws<ArgumentException>(() => stock.DeductStock(0));
        }

        [Fact]
        public void DeductStock_NegativeQuantity_ThrowsArgumentException()
        {
            var stock = new SkuStock("TX-001", 10);
            Assert.Throws<ArgumentException>(() => stock.DeductStock(-3));
        }

        [Fact]
        public void DeductStock_ErrorMessageContainsSku()
        {
            var stock = new SkuStock("TX-ERR", 2);
            var ex = Assert.Throws<InvalidOperationException>(() => stock.DeductStock(10));
            Assert.Contains("TX-ERR", ex.Message);
        }

        // --- AddStock ---

        [Fact]
        public void AddStock_ValidQuantity_IncreasesStock()
        {
            var stock = new SkuStock("TX-001", 10);
            stock.AddStock(5);
            Assert.Equal(15, stock.AvailableStock);
        }

        [Fact]
        public void AddStock_ZeroQuantity_ThrowsArgumentException()
        {
            var stock = new SkuStock("TX-001", 10);
            Assert.Throws<ArgumentException>(() => stock.AddStock(0));
        }

        [Fact]
        public void AddStock_NegativeQuantity_ThrowsArgumentException()
        {
            var stock = new SkuStock("TX-001", 10);
            Assert.Throws<ArgumentException>(() => stock.AddStock(-1));
        }

            [Fact]
        public void AddStock_LargeQuantity_WorksCorrectly()
        {
            var stock = new SkuStock("TX-001", 0);
            stock.AddStock(1000);
            Assert.Equal(1000, stock.AvailableStock);
        }

        [Fact]
        public void SkuStock_PrivateConstructor_CanBeInstantiatedViaReflection()
        {
            var type = typeof(SkuStock);
            var ctor = type.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            Assert.NotNull(ctor);
            var instance = ctor!.Invoke(null);
            Assert.NotNull(instance);
            Assert.IsType<SkuStock>(instance);
        }
    }

    // =========================================================================
    // InventoryItem Domain Tests
    // =========================================================================
    public class InventoryItemDomainTests
    {
        [Fact]
        public void InventoryItem_Constructor_SetsPropertiesCorrectly()
        {
            var id = Guid.NewGuid();
            var item = new InventoryItem(id, "tx-sku-01", 30);

            Assert.Equal(id, item.Id);
            Assert.Equal("TX-SKU-01", item.Sku); // ToUpper().Trim()
            Assert.Equal(30, item.AvailableStock);
            Assert.Equal(30, item.Quantity); // Computed property
            Assert.True(item.LastUpdatedAt <= DateTime.UtcNow);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void InventoryItem_Constructor_EmptySku_ThrowsArgumentException(string sku)
        {
            Assert.Throws<ArgumentException>(() => new InventoryItem(Guid.NewGuid(), sku, 10));
        }

        [Fact]
        public void InventoryItem_Constructor_NegativeStock_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new InventoryItem(Guid.NewGuid(), "TX-001", -1));
        }

        [Fact]
        public void InventoryItem_Constructor_ZeroStock_IsValid()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 0);
            Assert.Equal(0, item.AvailableStock);
        }

        [Fact]
        public void InventoryItem_SkuIsUppercasedAndTrimmed()
        {
            var item = new InventoryItem(Guid.NewGuid(), "  tx-sku  ", 5);
            Assert.Equal("TX-SKU", item.Sku);
        }

        [Fact]
        public void InventoryItem_Quantity_MatchesAvailableStock()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 42);
            Assert.Equal(item.AvailableStock, item.Quantity);
        }

        // --- UpdateQuantity ---

        [Fact]
        public void UpdateQuantity_ValidValue_UpdatesStock()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            item.UpdateQuantity(25);
            Assert.Equal(25, item.AvailableStock);
            Assert.Equal(25, item.Quantity);
        }

        [Fact]
        public void UpdateQuantity_ToZero_IsAllowed()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            item.UpdateQuantity(0);
            Assert.Equal(0, item.AvailableStock);
        }

        [Fact]
        public void UpdateQuantity_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => item.UpdateQuantity(-5));
        }

        [Fact]
        public void UpdateQuantity_UpdatesLastUpdatedAt()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            var before = item.LastUpdatedAt;
            System.Threading.Thread.Sleep(1);
            item.UpdateQuantity(20);
            Assert.True(item.LastUpdatedAt >= before);
        }

        // --- DeductStock ---

        [Fact]
        public void DeductStock_ValidQuantity_ReducesStock()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 20);
            item.DeductStock(7);
            Assert.Equal(13, item.AvailableStock);
        }

        [Fact]
        public void DeductStock_ExactStock_LeavesZero()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            item.DeductStock(10);
            Assert.Equal(0, item.AvailableStock);
        }

        [Fact]
        public void DeductStock_MoreThanAvailable_ThrowsInvalidOperationException()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 3);
            Assert.Throws<InvalidOperationException>(() => item.DeductStock(5));
        }

        [Fact]
        public void DeductStock_ZeroQuantity_ThrowsArgumentException()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            Assert.Throws<ArgumentException>(() => item.DeductStock(0));
        }

        [Fact]
        public void DeductStock_NegativeQuantity_ThrowsArgumentException()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            Assert.Throws<ArgumentException>(() => item.DeductStock(-2));
        }

        [Fact]
        public void DeductStock_UpdatesLastUpdatedAt()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-001", 10);
            var before = item.LastUpdatedAt;
            System.Threading.Thread.Sleep(1);
            item.DeductStock(3);
            Assert.True(item.LastUpdatedAt >= before);
        }

        [Fact]
        public void DeductStock_ErrorMessageContainsSku()
        {
            var item = new InventoryItem(Guid.NewGuid(), "TX-FAIL", 1);
            var ex = Assert.Throws<InvalidOperationException>(() => item.DeductStock(10));
            Assert.Contains("TX-FAIL", ex.Message);
        }
    }

    // =========================================================================
    // Private Constructor Coverage via Reflection (EF Core proxy support)
    // =========================================================================
    public class InventoryPrivateConstructorTests
    {
        [Fact]
        public void InventoryItem_PrivateParameterlessConstructor_CanBeInstantiatedViaReflection()
        {
            var type = typeof(InventoryItem);
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            Assert.NotNull(ctor);
            var instance = ctor!.Invoke(null);
            Assert.NotNull(instance);
            Assert.IsType<InventoryItem>(instance);
        }
    }

    // =========================================================================
    // Inventory Integration Events Tests
    // =========================================================================
    public class InventoryIntegrationEventTests
    {
        [Fact]
        public void OrderPlacedIntegrationEvent_Constructor_SetsAllProperties()
        {
            var orderId = Guid.NewGuid();
            var items = new List<OrderStockItemDto>
            {
                new OrderStockItemDto("TX-001", 5),
                new OrderStockItemDto("TX-002", 3)
            };

            var evt = new OrderPlacedIntegrationEvent(orderId, "customer-1", items);

            Assert.Equal(orderId, evt.OrderId);
            Assert.Equal("customer-1", evt.CustomerId);
            Assert.Equal(2, evt.Items.Count);
        }

        [Fact]
        public void OrderStockItemDto_Constructor_SetsAllProperties()
        {
            var dto = new OrderStockItemDto("TX-SKU", 10);

            Assert.Equal("TX-SKU", dto.Sku);
            Assert.Equal(10, dto.Quantity);
        }

        [Fact]
        public void OrderStockItemDto_RecordEquality_WorksCorrectly()
        {
            var dto1 = new OrderStockItemDto("TX-001", 5);
            var dto2 = new OrderStockItemDto("TX-001", 5);
            var dto3 = new OrderStockItemDto("TX-002", 5);

            Assert.Equal(dto1, dto2);
            Assert.NotEqual(dto1, dto3);
        }

        [Fact]
        public void InventoryReservedIntegrationEvent_Constructor_SetsAllProperties()
        {
            var orderId = Guid.NewGuid();
            var reservedAt = DateTime.UtcNow;

            var evt = new InventoryReservedIntegrationEvent(orderId, reservedAt);

            Assert.Equal(orderId, evt.OrderId);
            Assert.Equal(reservedAt, evt.ReservedAt);
        }

        [Fact]
        public void InventoryReservedIntegrationEvent_RecordEquality_WorksCorrectly()
        {
            var orderId = Guid.NewGuid();
            var at = DateTime.UtcNow;
            var evt1 = new InventoryReservedIntegrationEvent(orderId, at);
            var evt2 = new InventoryReservedIntegrationEvent(orderId, at);

            Assert.Equal(evt1, evt2);
        }

        [Fact]
        public void InventoryReservationFailedIntegrationEvent_Constructor_SetsAllProperties()
        {
            var orderId = Guid.NewGuid();
            var failedAt = DateTime.UtcNow;

            var evt = new InventoryReservationFailedIntegrationEvent(orderId, "Insufficient stock", failedAt);

            Assert.Equal(orderId, evt.OrderId);
            Assert.Equal("Insufficient stock", evt.Reason);
            Assert.Equal(failedAt, evt.FailedAt);
        }

        [Fact]
        public void InventoryReservationFailedIntegrationEvent_RecordEquality_WorksCorrectly()
        {
            var orderId = Guid.NewGuid();
            var at = DateTime.UtcNow;
            var evt1 = new InventoryReservationFailedIntegrationEvent(orderId, "No stock", at);
            var evt2 = new InventoryReservationFailedIntegrationEvent(orderId, "No stock", at);

            Assert.Equal(evt1, evt2);
        }
    }

    // =========================================================================
    // ProductStockUpdatedEvent Tests
    // =========================================================================
    public class ProductStockUpdatedEventTests
    {
        [Fact]
        public void ProductStockUpdatedEvent_Constructor_SetsAllProperties()
        {
            var evt = new ProductStockUpdatedEvent("TX-001", 42);

            Assert.Equal("TX-001", evt.Sku);
            Assert.Equal(42, evt.NewStock);
        }

        [Fact]
        public void ProductStockUpdatedEvent_RecordEquality_WorksCorrectly()
        {
            var evt1 = new ProductStockUpdatedEvent("TX-001", 42);
            var evt2 = new ProductStockUpdatedEvent("TX-001", 42);
            var evt3 = new ProductStockUpdatedEvent("TX-002", 42);

            Assert.Equal(evt1, evt2);
            Assert.NotEqual(evt1, evt3);
        }

        [Fact]
        public void ProductStockUpdatedEvent_DifferentStock_AreNotEqual()
        {
            var evt1 = new ProductStockUpdatedEvent("TX-001", 10);
            var evt2 = new ProductStockUpdatedEvent("TX-001", 20);

            Assert.NotEqual(evt1, evt2);
        }
    }
}
