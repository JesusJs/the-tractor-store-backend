using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using TractorEcommerce.Modules.Catalog.Application.Ports;
using TractorEcommerce.Modules.Catalog.Application.UseCase;
using TractorEcommerce.Modules.Catalog.Domain.Entities;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;
using Xunit;
using static TractorEcommerce.Modules.Catalog.Application.DTOs.CatalogDtos;

namespace TractorEcommerce.Modules.Catalog.Tests.Controllers
{
    public class InventoryControllerTests
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ILogger<InventoryController> _logger;
        private readonly InventoryController _controller;

        public InventoryControllerTests()
        {
            _catalogRepository = Substitute.For<ICatalogRepository>();
            _logger = Substitute.For<ILogger<InventoryController>>();
            var getInventoryStatus = new GetInventoryStatusUseCase(_catalogRepository);
            _controller = new InventoryController(getInventoryStatus, _logger);
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

            // Act & Assert
            await Assert.ThrowsAsync<DomainNotFoundException>(() => _controller.GetInventory(sku));
        }
    }
}
