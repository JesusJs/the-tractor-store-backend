using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using TractorEcommerce.Api.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using Xunit;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Tests.Controllers
{
    public class CatalogControllerTests
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly CatalogController _controller;

        public CatalogControllerTests()
        {
            _catalogRepository = Substitute.For<ICatalogRepository>();

            var getHomeTeasers = new GetHomeTeasersUseCase();
            var getCatalogCategory = new GetCatalogCategoryUseCase(_catalogRepository);
            var getProductDetail = new GetProductDetailUseCase(_catalogRepository);
            var getRecommendations = new GetRecommendationsUseCase(_catalogRepository);
            var getStores = new GetStoresUseCase(_catalogRepository);

            _controller = new CatalogController(
                getHomeTeasers,
                getCatalogCategory,
                getProductDetail,
                getRecommendations,
                getStores
            );
        }

        [Fact]
        public async Task GetHome_ShouldReturnOkWithTeasers()
        {
            // Act
            var result = await _controller.GetHome();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var teasers = Assert.IsAssignableFrom<IEnumerable<TeaserDto>>(okResult.Value);
            Assert.Equal(2, teasers.Count());
        }

        [Fact]
        public async Task GetCategory_ShouldReturnOkWithCategoryProducts()
        {
            // Arrange
            var filter = "autonomous";
            var product = new Product("tx-1", "Autonomous Tractor", "BrandX", 95000, "img", "Desc", "autonomous");
            product.AddVariant("TX-1-AUTO", 5);

            _catalogRepository.GetByCategoryAsync(filter)
                .Returns(new List<Product> { product });

            // Act
            var result = await _controller.GetCategory(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoryDto = Assert.IsType<CatalogCategoryDto>(okResult.Value);
            Assert.Equal(filter, categoryDto.Category);
            Assert.Single(categoryDto.Products);
            Assert.Equal("tx-1", categoryDto.Products.First().Id);
        }

        [Fact]
        public async Task GetProduct_WithExistingId_ShouldReturnOkProductDetail()
        {
            // Arrange
            var productId = "tx-1";
            var product = new Product(productId, "Classic Tractor", "BrandY", 45000, "img", "Desc", "classics");
            product.AddVariant("TX-1-VAR", 10);
            product.AddHighlight("Fast");

            _catalogRepository.GetByIdAsync(productId)
                .Returns(product);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var detailDto = Assert.IsType<ProductDetailDto>(okResult.Value);
            Assert.Equal(productId, detailDto.Id);
            Assert.Contains("TX-1-VAR", detailDto.Variants);
            Assert.Contains("Fast", detailDto.Highlights);
        }

        [Fact]
        public async Task GetProduct_WithNonExistingId_ShouldThrowDomainNotFoundException()
        {
            // Arrange
            var productId = "missing-id";
            _catalogRepository.GetByIdAsync(productId)
                .Returns((Product?)null);

            // Act & Assert
            await Assert.ThrowsAsync<DomainNotFoundException>(() => _controller.GetProduct(productId));
        }

        [Fact]
        public async Task GetRecommendations_ShouldReturnOkRecommendations()
        {
            // Arrange
            var skus = "TX-1-VAR";
            var product = new Product("tx-1", "Classic Tractor", "BrandY", 45000, "img", "Desc", "classics");
            product.AddVariant("TX-1-VAR", 10);

            _catalogRepository.GetProductsBySkusAsync(Arg.Any<IEnumerable<string>>())
                .Returns(new List<Product> { product });
            _catalogRepository.GetByCategoryAsync("classics")
                .Returns(new List<Product>());

            // Act
            var result = await _controller.GetRecommendations(skus);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<IEnumerable<ProductItemDto>>(okResult.Value);
        }

        [Fact]
        public async Task GetStores_ShouldReturnOkStoresList()
        {
            // Arrange
            var store = new Store("st-1", "Store One", "123 Main St", "CityA", "img");
            _catalogRepository.GetStoresAsync()
                .Returns(new List<Store> { store });

            // Act
            var result = await _controller.GetStores();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var storesList = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(storesList);
        }
    }
}
