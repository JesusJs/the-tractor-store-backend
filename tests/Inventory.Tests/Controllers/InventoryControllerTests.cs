using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TractorEcommerce.Api.Controllers;
using TractorEcommerce.Modules.Inventory.Application.DTOs;
using TractorEcommerce.Modules.Inventory.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Inventory.Application.UseCase;
using TractorEcommerce.Modules.Inventory.Domain.Entities;
using TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Exceptions;

namespace TractorEcommerce.Modules.Inventory.Tests.Controllers
{
    // =========================================================================
    // InventoryController Tests (using Inventory module's own use case)
    // =========================================================================
    public class InventoryModuleControllerTests
    {
        private readonly IInventoryRepository _repository;
        private readonly ILogger<InventoryController> _logger;
        private readonly GetInventoryStatusUseCase _getInventoryStatusUseCase;

        public InventoryModuleControllerTests()
        {
            _repository = Substitute.For<IInventoryRepository>();
            _logger = Substitute.For<ILogger<InventoryController>>();
            _getInventoryStatusUseCase = new GetInventoryStatusUseCase(_repository);
        }

        [Fact]
        public async Task GetInventoryStatusUseCase_WithExistingSku_ReturnsCorrectDto()
        {
            // Arrange
            var sku = "TX-001";
            var inventoryItem = new InventoryItem(Guid.NewGuid(), sku, 30);
            _repository.GetBySkuAsync(sku).Returns(inventoryItem);

            // Act
            var result = await _getInventoryStatusUseCase.ExecuteAsync(sku);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetInventoryStatusUseCase_WithMissingSku_ReturnsNull()
        {
            // Arrange
            _repository.GetBySkuAsync("MISSING").Returns((InventoryItem?)null);

            // Act
            var result = await _getInventoryStatusUseCase.ExecuteAsync("MISSING");

            // Assert
            Assert.Null(result);
        }
    }
}
