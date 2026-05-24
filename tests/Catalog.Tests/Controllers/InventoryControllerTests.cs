using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using Xunit;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Tests.Controllers
{
    public class InventoryControllerTests
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly InventoryController _controller;

        public InventoryControllerTests()
        {
            _catalogRepository = Substitute.For<ICatalogRepository>();
            var getInventoryStatus = new GetInventoryStatusUseCase(_catalogRepository);
            _controller = new InventoryController(getInventoryStatus);
        }

        [Fact]
        public async Task GetInventory_WithExistingSku_ShouldReturnOkWithStock()
        {
            // Arrange
            var sku = "TX-1-VAR";
            var variant = new ProductVariant(sku, "tx-1", 15);
            _catalogRepository.GetVariantBySkuAsync(sku).Returns(variant);

            // Act
            var result = await _controller.GetInventory(sku);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var status = Assert.IsType<InventoryStatusDto>(okResult.Value);
            Assert.Equal(sku, status.Sku);
            Assert.Equal(15, status.Stock);
        }

        [Fact]
        public async Task GetInventory_WithNonExistingSku_ShouldReturnNotFound()
        {
            // Arrange
            var sku = "MISSING-SKU";
            _catalogRepository.GetVariantBySkuAsync(sku).Returns((ProductVariant?)null);

            // Act
            var result = await _controller.GetInventory(sku);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }
}
