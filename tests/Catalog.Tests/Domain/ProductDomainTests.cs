using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Catalog.Domain.Entities;

namespace TractorEcommerce.Modules.Catalog.Tests.Domain
{
    public class ProductDomainTests
    {
    [Fact]
        public void CreateProduct_WithValidData_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var product = new Product("tx-001", "Tractor Autónomo", "John Deere", 85000, "url", "Descripción", "autonomous", "240 HP");

            // Assert
            Assert.Equal("tx-001", product.Id);
            Assert.Equal("autonomous", product.Category);
            Assert.Empty(product.Variants);
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
    }
}
