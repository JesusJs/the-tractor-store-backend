using System;
using System.Collections.Generic;
using TractorEcommerce.Modules.Catalog.Domain.Entities;

namespace TractorEcommerce.Modules.Catalog.Tests.Domain
{
    // =========================================================================
    // Product Domain Tests
    // =========================================================================
    public class ProductDomainTests
    {
        // --- Constructor ---

        [Fact]
        public void CreateProduct_WithValidData_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var product = new Product("tx-001", "Tractor Autónomo", "John Deere", 85000, "url", "Descripción", "autonomous", "240 HP");

            // Assert
            Assert.Equal("tx-001", product.Id);
            Assert.Equal("Tractor Autónomo", product.Name);
            Assert.Equal("John Deere", product.Brand);
            Assert.Equal(85000, product.Price);
            Assert.Equal("url", product.Image);
            Assert.Equal("Descripción", product.Description);
            Assert.Equal("autonomous", product.Category); // ToLower()
            Assert.Equal("240 HP", product.EnginePower);
            Assert.Empty(product.Variants);
            Assert.Empty(product.Highlights);
        }

        [Fact]
        public void CreateProduct_WithoutEnginePower_ShouldHaveNullEnginePower()
        {
            var product = new Product("tx-002", "Classic", "BrandA", 30000, "img", "Desc", "classics");
            Assert.Null(product.EnginePower);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateProduct_WithEmptyId_ShouldThrowArgumentException(string id)
        {
            Assert.Throws<ArgumentException>(() =>
                new Product(id, "Name", "Brand", 50000, "img", "Desc", "all"));
        }

        [Fact]
        public void CreateProduct_WithNegativePrice_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Product("tx-003", "Tractor", "Brand", -100, "img", "Desc", "all"));
        }

        [Fact]
        public void CreateProduct_WithZeroPrice_ShouldBeValid()
        {
            var product = new Product("tx-004", "Tractor", "Brand", 0, "img", "Desc", "all");
            Assert.Equal(0, product.Price);
        }

        [Fact]
        public void CreateProduct_CategoryIsLowercased()
        {
            var product = new Product("tx-005", "Name", "Brand", 1000, "img", "Desc", "CLASSICS");
            Assert.Equal("classics", product.Category);
        }

        // --- AddVariant ---

        [Fact]
        public void AddVariant_ValidSku_ShouldAddVariantCorrectly()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 85000, "url", "Desc", "all");
            product.AddVariant("TX-001-GPS", 10);

            Assert.Single(product.Variants);
        }

        [Fact]
        public void AddVariant_WithNegativeStock_ShouldThrowArgumentException()
        {
            // Arrange
            var product = new Product("tx-001", "Tractor", "Brand", 85000, "url", "Desc", "all");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => product.AddVariant("SKU-ERR", -5));
        }

        [Fact]
        public void AddVariant_ExistingSku_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var product = new Product("tx-001", "Tractor", "Brand", 85000, "url", "Desc", "all");
            product.AddVariant("TX-001-GPS", 10);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => product.AddVariant("TX-001-GPS", 5));
        }

        [Fact]
        public void AddVariant_MultipleDistinctSkus_ShouldAddAllVariants()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 50000, "url", "Desc", "all");
            product.AddVariant("SKU-A", 5);
            product.AddVariant("SKU-B", 3);
            product.AddVariant("SKU-C", 10);

            Assert.Equal(3, product.Variants.Count);
        }

        [Fact]
        public void AddVariant_WithZeroStock_ShouldBeValid()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 50000, "url", "Desc", "all");
            product.AddVariant("TX-ZERO", 0);

            Assert.Single(product.Variants);
        }

        // --- AddHighlight ---

        [Fact]
        public void AddHighlight_ValidHighlight_ShouldAdd()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 50000, "url", "Desc", "all");
            product.AddHighlight("Electric Start");

            Assert.Single(product.Highlights);
            Assert.Contains("Electric Start", product.Highlights);
        }

        [Fact]
        public void AddHighlight_NullOrWhitespace_ShouldNotAdd()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 50000, "url", "Desc", "all");
            product.AddHighlight("");
            product.AddHighlight("   ");
            product.AddHighlight(null!);

            Assert.Empty(product.Highlights);
        }

        [Fact]
        public void AddHighlight_MultipleHighlights_ShouldAddAll()
        {
            var product = new Product("tx-001", "Tractor", "Brand", 50000, "url", "Desc", "all");
            product.AddHighlight("GPS Precision");
            product.AddHighlight("Auto Steering");
            product.AddHighlight("Solar Panel");

            Assert.Equal(3, product.Highlights.Count);
        }
    }

    // =========================================================================
    // ProductVariant Domain Tests
    // =========================================================================
    public class ProductVariantDomainTests
    {
        [Fact]
        public void ProductVariant_Constructor_SetsPropertiesCorrectly()
        {
            var variant = new ProductVariant("TX-001-STD", "tx-001", 15);

            Assert.Equal("TX-001-STD", variant.Sku);
            Assert.Equal("tx-001", variant.ProductId);
            Assert.Equal(15, variant.Stock);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ProductVariant_EmptySku_ThrowsArgumentException(string sku)
        {
            Assert.Throws<ArgumentException>(() => new ProductVariant(sku, "tx-001", 10));
        }

        [Fact]
        public void ProductVariant_NegativeStock_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new ProductVariant("TX-001-STD", "tx-001", -1));
        }

        [Fact]
        public void ProductVariant_ZeroStock_IsValid()
        {
            var variant = new ProductVariant("TX-001-STD", "tx-001", 0);
            Assert.Equal(0, variant.Stock);
        }

        [Fact]
        public void UpdateStock_ValidNewStock_UpdatesStock()
        {
            var variant = new ProductVariant("TX-001-STD", "tx-001", 10);
            variant.UpdateStock(25);
            Assert.Equal(25, variant.Stock);
        }

        [Fact]
        public void UpdateStock_ToZero_IsAllowed()
        {
            var variant = new ProductVariant("TX-001-STD", "tx-001", 10);
            variant.UpdateStock(0);
            Assert.Equal(0, variant.Stock);
        }

        [Fact]
        public void UpdateStock_NegativeStock_ThrowsArgumentException()
        {
            var variant = new ProductVariant("TX-001-STD", "tx-001", 10);
            Assert.Throws<ArgumentException>(() => variant.UpdateStock(-5));
        }
    }

    // =========================================================================
    // Store Domain Tests
    // =========================================================================
    public class StoreDomainTests
    {
        [Fact]
        public void Store_Constructor_SetsAllPropertiesCorrectly()
        {
            var store = new Store("st-1", "Main Store", "Calle 1 #2-3", "Bogotá", "img.jpg");

            Assert.Equal("st-1", store.Id);
            Assert.Equal("Main Store", store.Name);
            Assert.Equal("Calle 1 #2-3", store.Address);
            Assert.Equal("Bogotá", store.City);
            Assert.Equal("img.jpg", store.Image);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Store_Constructor_EmptyId_ThrowsArgumentException(string id)
        {
            Assert.Throws<ArgumentException>(() => new Store(id, "Store Name", "Address", "City", "img"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Store_Constructor_EmptyName_ThrowsArgumentException(string name)
        {
            Assert.Throws<ArgumentException>(() => new Store("st-1", name, "Address", "City", "img"));
        }

        [Fact]
        public void Store_Constructor_NullAddress_IsValid()
        {
            // Address/City/Image can be null or empty; only Id and Name are validated
            var store = new Store("st-2", "Uptown Store", null!, "City", null!);
            Assert.Equal("st-2", store.Id);
        }
    }
}
