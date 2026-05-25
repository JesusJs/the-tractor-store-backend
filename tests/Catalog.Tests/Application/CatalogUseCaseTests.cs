using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using Xunit;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;
using TractorEcommerce.Modules.Catalog.Application.Events;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Events;

namespace TractorEcommerce.Modules.Catalog.Tests.Application
{
    // ---------------------------------------------------------------------------
    // GetCatalogCategoryUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetCatalogCategoryUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly GetCatalogCategoryUseCase _useCase;

        public GetCatalogCategoryUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new GetCatalogCategoryUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithProducts_ReturnsCategoryDto()
        {
            // Arrange
            var category = "classics";
            var product = new Product("tx-1", "Classic Tractor", "BrandY", 45000, "img", "Desc", category);
            product.AddVariant("TX-1-VAR", 10);
            _repo.GetByCategoryAsync(category).Returns(new List<Product> { product });

            // Act
            var result = await _useCase.ExecuteAsync(category);

            // Assert
            Assert.Equal(category, result.Category);
            Assert.Single(result.Products);
            Assert.Equal("tx-1", result.Products.First().Id);
        }

        [Fact]
        public async Task Execute_WithNoProducts_ReturnsEmptyList()
        {
            // Arrange
            var category = "electric";
            _repo.GetByCategoryAsync(category).Returns(new List<Product>());

            // Act
            var result = await _useCase.ExecuteAsync(category);

            // Assert
            Assert.Equal(category, result.Category);
            Assert.Empty(result.Products);
        }
    }

    // ---------------------------------------------------------------------------
    // GetProductDetailUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetProductDetailUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly GetProductDetailUseCase _useCase;

        public GetProductDetailUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new GetProductDetailUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithExistingProduct_ReturnsDetail()
        {
            // Arrange
            var product = new Product("tx-1", "Classic Tractor", "BrandY", 45000, "img", "Desc", "classics");
            product.AddVariant("TX-1-STD", 8);
            product.AddVariant("TX-1-GPS", 3);
            product.AddHighlight("Electric Start");
            _repo.GetByIdAsync("tx-1").Returns(product);

            // Act
            var result = await _useCase.ExecuteAsync("tx-1");

            // Assert
            Assert.Equal("tx-1", result.Id);
            Assert.Equal("Classic Tractor", result.Name);
            Assert.Contains("TX-1-STD", result.Variants);
            Assert.Contains("TX-1-GPS", result.Variants);
            Assert.Contains("Electric Start", result.Highlights);
        }

        [Fact]
        public async Task Execute_WithNonExistingProduct_ReturnsNull()
        {
            // Arrange
            // Note: DomainNotFoundException is thrown by the Controller, not by this use case.
            // The use case returns null when the product is not found.
            _repo.GetByIdAsync("missing").Returns((Product?)null);

            // Act
            var result = await _useCase.ExecuteAsync("missing");

            // Assert
            Assert.Null(result);
        }
    }

    // ---------------------------------------------------------------------------
    // GetStoresUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetStoresUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly GetStoresUseCase _useCase;

        public GetStoresUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new GetStoresUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithStores_ReturnsStoreDtos()
        {
            // Arrange
            var stores = new List<Store>
            {
                new Store("st-1", "Main Store",  "Calle Mayor 1", "Madrid", "img1"),
                new Store("st-2", "North Store", "Avenida Norte 2", "Barcelona", "img2"),
            };
            _repo.GetStoresAsync().Returns(stores);

            // Act
            // ExecuteAsync returns IEnumerable<object> with anonymous type projections
            var result = (await _useCase.ExecuteAsync()).ToList();

            // Assert — 2 stores projected
            Assert.Equal(2, result.Count);
            Assert.Equal("st-1", result[0].Id);
            Assert.Equal("Main Store", result[0].Name);
            Assert.Equal("Calle Mayor 1", result[0].Address);
            Assert.Equal("Madrid", result[0].City);
            Assert.Equal("img1", result[0].Image);
        }

        [Fact]
        public async Task Execute_WithNoStores_ReturnsEmptyList()
        {
            // Arrange
            _repo.GetStoresAsync().Returns(new List<Store>());

            // Act
            var result = await _useCase.ExecuteAsync();

            // Assert
            Assert.Empty(result);
        }
    }

    // ---------------------------------------------------------------------------
    // GetInventoryStatusUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetInventoryStatusUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly GetInventoryStatusUseCase _useCase;

        public GetInventoryStatusUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new GetInventoryStatusUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithExistingVariant_ReturnsInventoryStatus()
        {
            // Arrange
            var sku = "TX-1-STD";
            _repo.GetVariantBySkuAsync(sku).Returns(new ProductVariant(sku, "tx-1", 42));

            // Act
            var result = await _useCase.ExecuteAsync(sku);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sku, result!.Sku);
            Assert.Equal(42, result.Stock);
            Assert.True(result.Stock > 0); // Inferred availability from stock
        }

        [Fact]
        public async Task Execute_WithMissingVariant_ReturnsNull()
        {
            // Arrange
            _repo.GetVariantBySkuAsync("MISSING").Returns((ProductVariant?)null);

            // Act
            var result = await _useCase.ExecuteAsync("MISSING");

            // Assert
            Assert.Null(result);
        }
    }

    // ---------------------------------------------------------------------------
    // DecreaseStockUseCase Tests
    // ---------------------------------------------------------------------------
    public class DecreaseStockUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly DecreaseStockUseCase _useCase;

        public DecreaseStockUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new DecreaseStockUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithExistingVariant_UpdatesStockAndSaves()
        {
            // Arrange
            var sku = "TX-2-GPS";
            var variant = new ProductVariant(sku, "tx-2", 10);
            _repo.GetVariantBySkuAsync(sku).Returns(variant);

            // Act
            await _useCase.ExecuteAsync(sku, 3);

            // Assert — stock must be 10 - 3 = 7
            Assert.Equal(7, variant.Stock);
            await _repo.Received(1).UpdateVariantAsync(variant);
        }

        [Fact]
        public async Task Execute_WithMissingVariant_DoesNotCallUpdate()
        {
            // Arrange
            _repo.GetVariantBySkuAsync("GHOST").Returns((ProductVariant?)null);

            // Act — should not throw
            await _useCase.ExecuteAsync("GHOST", 5);

            // Assert — UpdateVariantAsync must NOT be called
            await _repo.DidNotReceive().UpdateVariantAsync(Arg.Any<ProductVariant>());
        }

        [Fact]
        public async Task Execute_DecreaseByFullStock_LeavesZero()
        {
            // Arrange
            var sku = "TX-3-STD";
            var variant = new ProductVariant(sku, "tx-3", 5);
            _repo.GetVariantBySkuAsync(sku).Returns(variant);

            // Act
            await _useCase.ExecuteAsync(sku, 5);

            // Assert
            Assert.Equal(0, variant.Stock);
        }

        [Fact]
        public async Task Execute_DecreaseByOne_CorrectlyDecrements()
        {
            // Arrange
            var sku = "TX-4-AUTO";
            var variant = new ProductVariant(sku, "tx-4", 1);
            _repo.GetVariantBySkuAsync(sku).Returns(variant);

            // Act
            await _useCase.ExecuteAsync(sku, 1);

            // Assert
            Assert.Equal(0, variant.Stock);
            await _repo.Received(1).UpdateVariantAsync(variant);
        }
    }

    // ---------------------------------------------------------------------------
    // GetRecommendationsUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetRecommendationsUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly GetRecommendationsUseCase _useCase;

        public GetRecommendationsUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _useCase = new GetRecommendationsUseCase(_repo);
        }

        [Fact]
        public async Task Execute_WithMatchingSkus_ReturnsRelatedProducts()
        {
            // Arrange — SKUs as comma-separated string (the real signature)
            var skus = "TX-1-STD";
            var matchedProduct = new Product("tx-1", "Classic Tractor", "BrandY", 45000, "img", "Desc", "classics");
            matchedProduct.AddVariant("TX-1-STD", 5);

            var relatedProduct = new Product("tx-2", "Retro Tractor", "BrandZ", 38000, "img2", "Desc2", "classics");
            relatedProduct.AddVariant("TX-2-STD", 3);

            _repo.GetProductsBySkusAsync(Arg.Any<IEnumerable<string>>())
                 .Returns(new List<Product> { matchedProduct });
            // same-category products (excluding the matched one)
            _repo.GetByCategoryAsync("classics")
                 .Returns(new List<Product> { matchedProduct, relatedProduct });

            // Act
            var result = (await _useCase.ExecuteAsync(skus)).ToList();

            // Assert — should include related (not matched) products
            Assert.NotEmpty(result);
            Assert.Contains(result, r => r.Id == "tx-2");
            Assert.DoesNotContain(result, r => r.Id == "tx-1"); // matched product excluded
        }

        [Fact]
        public async Task Execute_WithNullSkus_ReturnsCategoryFallback()
        {
            // Arrange
            var fallbackProduct = new Product("tx-5", "All Terrain", "BrandQ", 25000, "img5", "Desc5", "all");
            fallbackProduct.AddVariant("TX-5-STD", 2);

            _repo.GetByCategoryAsync("all").Returns(new List<Product> { fallbackProduct });

            // Act
            var result = (await _useCase.ExecuteAsync(null)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("tx-5", result[0].Id);
        }

        [Fact]
        public async Task Execute_WithNonMatchingSkus_ReturnsFallbackAll()
        {
            // Arrange — skus that don't match any product
            var skus = "UNKNOWN-SKU";
            _repo.GetProductsBySkusAsync(Arg.Any<IEnumerable<string>>())
                 .Returns(new List<Product>()); // no match

            var fallbackProduct = new Product("tx-fb", "Fallback Tractor", "FallbackBrand", 20000, "img-fb", "Desc-fb", "all");
            fallbackProduct.AddVariant("TX-FB-STD", 1);
            _repo.GetByCategoryAsync("all").Returns(new List<Product> { fallbackProduct });

            // Act
            var result = (await _useCase.ExecuteAsync(skus)).ToList();

            // Assert — should fall back to "all" category
            Assert.Single(result);
            Assert.Equal("tx-fb", result[0].Id);
        }

        [Fact]
        public async Task Execute_WithEmptyString_ReturnsCategoryFallback()
        {
            // Arrange
            var fallbackProduct = new Product("tx-e", "Empty Tractor", "BrandE", 15000, "img-e", "Desc-e", "all");
            fallbackProduct.AddVariant("TX-E-STD", 4);
            _repo.GetByCategoryAsync("all").Returns(new List<Product> { fallbackProduct });

            // Act — empty string is treated as no skus
            var result = (await _useCase.ExecuteAsync("")).ToList();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task Execute_WithMultipleSkusSameCategory_ReturnsFilteredResults()
        {
            // Arrange — two products with same category
            var skus = "TX-A,TX-B";
            var prodA = new Product("tx-a", "Tractor A", "BrandA", 30000, "img-a", "Desc-a", "modern");
            prodA.AddVariant("TX-A", 5);
            var prodB = new Product("tx-b", "Tractor B", "BrandB", 35000, "img-b", "Desc-b", "modern");
            prodB.AddVariant("TX-B", 3);
            var prodC = new Product("tx-c", "Tractor C", "BrandC", 40000, "img-c", "Desc-c", "modern");
            prodC.AddVariant("TX-C", 2);

            _repo.GetProductsBySkusAsync(Arg.Any<IEnumerable<string>>())
                 .Returns(new List<Product> { prodA, prodB });
            _repo.GetByCategoryAsync("modern")
                 .Returns(new List<Product> { prodA, prodB, prodC });

            // Act
            var result = (await _useCase.ExecuteAsync(skus)).ToList();

            // Assert — only prodC (the one NOT matched)
            Assert.Single(result);
            Assert.Equal("tx-c", result[0].Id);
        }
    }

    // ---------------------------------------------------------------------------
    // GetHomeTeasersUseCase Tests
    // ---------------------------------------------------------------------------
    public class GetHomeTeasersUseCaseTests
    {
        private readonly GetHomeTeasersUseCase _useCase;

        public GetHomeTeasersUseCaseTests()
        {
            _useCase = new GetHomeTeasersUseCase();
        }

        [Fact]
        public async Task Execute_ReturnsTeasers()
        {
            // Act
            var result = (await _useCase.ExecuteAsync()).ToList();

            // Assert — the use case returns hardcoded teasers
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task Execute_ReturnsAtLeastTwoTeasers()
        {
            // Act
            var result = (await _useCase.ExecuteAsync()).ToList();

            // Assert
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public async Task Execute_TeaserHasRequiredFields()
        {
            // Act
            var result = (await _useCase.ExecuteAsync()).ToList();

            // Assert — each teaser should have non-empty Id, Title, Image, Filter
            foreach (var teaser in result)
            {
                Assert.False(string.IsNullOrWhiteSpace(teaser.Id));
                Assert.False(string.IsNullOrWhiteSpace(teaser.Title));
                Assert.False(string.IsNullOrWhiteSpace(teaser.Image));
                Assert.False(string.IsNullOrWhiteSpace(teaser.Filter));
            }
        }
    }

    // ---------------------------------------------------------------------------
    // UpdateProductStockUseCase Tests
    // ---------------------------------------------------------------------------
    public class UpdateProductStockUseCaseTests
    {
        private readonly ICatalogRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly UpdateProductStockUseCase _useCase;

        public UpdateProductStockUseCaseTests()
        {
            _repo = Substitute.For<ICatalogRepository>();
            _eventBus = Substitute.For<IEventBus>();
            _useCase = new UpdateProductStockUseCase(_repo, _eventBus);
        }

        [Fact]
        public async Task Execute_WithExistingVariant_UpdatesStockAndPublishesEvent()
        {
            // Arrange
            var sku = "TX-VAR-1";
            var variant = new ProductVariant(sku, "tx-1", 10);
            _repo.GetVariantBySkuAsync(sku).Returns(variant);

            // Act
            await _useCase.ExecuteAsync(sku, 5);

            // Assert
            Assert.Equal(15, variant.Stock);
            await _repo.Received(1).UpdateVariantAsync(variant);
            await _eventBus.Received(1).PublishAsync(
                "catalog.products.stock-updated",
                sku,
                Arg.Is<ProductStockUpdatedEvent>(e => e.Sku == sku && e.NewStock == 15)
            );
        }

        [Fact]
        public async Task Execute_WithMissingVariant_DoesNotCallUpdateOrPublish()
        {
            // Arrange
            var sku = "MISSING-SKU";
            _repo.GetVariantBySkuAsync(sku).Returns((ProductVariant?)null);

            // Act
            await _useCase.ExecuteAsync(sku, 5);

            // Assert
            await _repo.DidNotReceive().UpdateVariantAsync(Arg.Any<ProductVariant>());
            await _eventBus.DidNotReceive().PublishAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<object>()
            );
        }
    }
}
