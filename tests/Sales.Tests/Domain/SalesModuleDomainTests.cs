using System;
using System.Collections.Generic;
using System.Linq;
using TractorEcommerce.Modules.Sales.Domain.Entities;

namespace TractorEcommerce.Modules.Sales.Tests.Domain
{
    // =========================================================================
    // SalesDomain Tests — Cart, CartItem, OrderReceipt, OrderReceiptItem
    // =========================================================================

    // Note: The existing SalesDomainTests.cs in this folder covers Cart (Cart.Module domain) and CustomerOrder.
    // These tests cover the Sales module's own domain entities.

    public class SalesCartItemDomainTests
    {
        [Fact]
        public void CartItem_Constructor_SetsAllPropertiesCorrectly()
        {
            var item = new CartItem("prod-1", "SKU-001", "Tractor A", "Standard", 5000m, "img.jpg");

            Assert.Equal("prod-1", item.ProductId);
            Assert.Equal("SKU-001", item.VariantId);
            Assert.Equal("Tractor A", item.ProductName);
            Assert.Equal("Standard", item.VariantName);
            Assert.Equal(5000m, item.Price);
            Assert.Equal("img.jpg", item.Image);
            Assert.Equal(1, item.Quantity); // Default quantity
        }

        [Fact]
        public void CartItem_Constructor_WithExplicitQuantity_SetsQuantity()
        {
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "Variant", 2000m, "img.jpg", quantity: 5);

            Assert.Equal(5, item.Quantity);
        }

        [Fact]
        public void CartItem_IncrementQuantity_IncreasesByOne()
        {
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "Std", 100m, "img");
            var originalQuantity = item.Quantity;

            item.IncrementQuantity();

            Assert.Equal(originalQuantity + 1, item.Quantity);
        }

        [Fact]
        public void CartItem_IncrementQuantity_MultipleTimes_AccumulatesCorrectly()
        {
            var item = new CartItem("prod-1", "SKU-001", "Tractor", "Std", 100m, "img");

            item.IncrementQuantity();
            item.IncrementQuantity();
            item.IncrementQuantity();

            Assert.Equal(4, item.Quantity); // 1 (default) + 3
        }
    }

    public class SalesCartDomainTests
    {
        [Fact]
        public void Cart_Constructor_SetsUserIdCorrectly()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-100");

            Assert.Equal("user-100", cart.UserId);
            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Cart_Constructor_NullUserId_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TractorEcommerce.Modules.Sales.Domain.Entities.Cart(null!));
        }

        [Fact]
        public void Cart_AddItem_NewItem_IsAddedCorrectly()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");

            Assert.Single(cart.Items);
            var item = cart.Items.First();
            Assert.Equal("SKU-A", item.VariantId);
            Assert.Equal(1, item.Quantity);
        }

        [Fact]
        public void Cart_AddItem_ExistingSku_IncrementsQuantity()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");

            Assert.Single(cart.Items);
            Assert.Equal(2, cart.Items.First().Quantity);
        }

        [Fact]
        public void Cart_AddItem_DifferentSkus_AddsMultiple()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor A", "Std", 3000m, "img");
            cart.AddItem("prod-2", "SKU-B", "Tractor B", "GPS", 5000m, "img2");

            Assert.Equal(2, cart.Items.Count);
        }

        [Fact]
        public void Cart_RemoveItem_ExistingSku_RemovesIt()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");
            cart.RemoveItem("SKU-A");

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Cart_RemoveItem_NonExistingSku_DoesNotThrow()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            // Should not throw
            cart.RemoveItem("NONEXISTENT");
            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Cart_Clear_RemovesAllItems()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");
            cart.AddItem("prod-2", "SKU-B", "Tractor B", "GPS", 5000m, "img2");
            cart.Clear();

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void Cart_TotalItems_CalculatesCorrectly()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 3000m, "img"); // +1
            cart.AddItem("prod-2", "SKU-B", "Tractor B", "GPS", 5000m, "img2");

            Assert.Equal(3, cart.TotalItems); // SKU-A x2 + SKU-B x1
        }

        [Fact]
        public void Cart_SubTotal_CalculatesCorrectly()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 1000m, "img"); // 1000 * 1

            Assert.Equal(1000m, cart.SubTotal);
        }

        [Fact]
        public void Cart_Tax_Is21Percent()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 1000m, "img");

            Assert.Equal(Math.Round(1000m * 0.21m, 2), cart.Tax);
        }

        [Fact]
        public void Cart_Total_IsSubTotalPlusTax()
        {
            var cart = new TractorEcommerce.Modules.Sales.Domain.Entities.Cart("user-1");
            cart.AddItem("prod-1", "SKU-A", "Tractor", "Std", 1000m, "img");

            Assert.Equal(cart.SubTotal + cart.Tax, cart.Total);
        }
    }

    public class OrderReceiptDomainTests
    {
        [Fact]
        public void OrderReceipt_Constructor_SetsAllPropertiesCorrectly()
        {
            var placedAt = DateTime.UtcNow;
            var receipt = new OrderReceipt(
                "ORD-001", "John", "Doe", "store-1", "pickup-1",
                200m, 42m, 242m, placedAt);

            Assert.Equal("ORD-001", receipt.Id);
            Assert.Equal("John", receipt.FirstName);
            Assert.Equal("Doe", receipt.LastName);
            Assert.Equal("store-1", receipt.StoreId);
            Assert.Equal("pickup-1", receipt.ExtraPickups);
            Assert.Equal(200m, receipt.SubTotal);
            Assert.Equal(42m, receipt.Tax);
            Assert.Equal(242m, receipt.Total);
            Assert.Equal(placedAt, receipt.PlacedAt);
            Assert.Empty(receipt.Items);
        }

        [Fact]
        public void OrderReceipt_Constructor_NullExtraPickups_IsValid()
        {
            var receipt = new OrderReceipt("ORD-002", "Jane", "Smith", "store-2", null, 100m, 19m, 119m, DateTime.UtcNow);
            Assert.Null(receipt.ExtraPickups);
        }

        [Fact]
        public void OrderReceipt_AddItem_AddsItemCorrectly()
        {
            var receipt = new OrderReceipt("ORD-003", "A", "B", "store-1", null, 100m, 19m, 119m, DateTime.UtcNow);
            var item = new OrderReceiptItem("SKU-1", "prod-1", "Tractor", "Std", 100m, 1, "img");
            receipt.AddItem(item);

            Assert.Single(receipt.Items);
            Assert.Same(item, receipt.Items.First());
        }

        [Fact]
        public void OrderReceipt_AddItem_MultipleItems_AddsAll()
        {
            var receipt = new OrderReceipt("ORD-004", "A", "B", "store-1", null, 300m, 57m, 357m, DateTime.UtcNow);
            receipt.AddItem(new OrderReceiptItem("SKU-1", "prod-1", "Tractor A", "Std", 100m, 1, "img1"));
            receipt.AddItem(new OrderReceiptItem("SKU-2", "prod-2", "Tractor B", "GPS", 200m, 1, "img2"));

            Assert.Equal(2, receipt.Items.Count);
        }
    }

    public class OrderReceiptItemDomainTests
    {
        [Fact]
        public void OrderReceiptItem_Constructor_SetsAllPropertiesCorrectly()
        {
            var item = new OrderReceiptItem("SKU-001", "prod-001", "Tractor X", "GPS", 15000m, 2, "img.jpg");

            Assert.Equal("SKU-001", item.Sku);
            Assert.Equal("prod-001", item.ProductId);
            Assert.Equal("Tractor X", item.ProductName);
            Assert.Equal("GPS", item.VariantName);
            Assert.Equal(15000m, item.Price);
            Assert.Equal(2, item.Quantity);
            Assert.Equal("img.jpg", item.Image);
        }

        [Fact]
        public void OrderReceiptItem_DifferentItems_HaveDifferentValues()
        {
            var item1 = new OrderReceiptItem("SKU-A", "p1", "Prod A", "Std", 100m, 1, "img1");
            var item2 = new OrderReceiptItem("SKU-B", "p2", "Prod B", "GPS", 200m, 3, "img2");

            Assert.NotEqual(item1.Sku, item2.Sku);
            Assert.NotEqual(item1.Price, item2.Price);
            Assert.NotEqual(item1.Quantity, item2.Quantity);
        }
    }
}
