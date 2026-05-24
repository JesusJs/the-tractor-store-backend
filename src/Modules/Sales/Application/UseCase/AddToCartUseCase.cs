using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Service;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.UseCase
{
    public record AddToCartCommand(
        string UserId,
        string Sku
    );

    public class AddToCartUseCase
    {
        private readonly ISalesRepository _salesRepository;
        private readonly ICatalogService _catalogService;

        public AddToCartUseCase(ISalesRepository salesRepository, ICatalogService catalogService)
        {
            _salesRepository = salesRepository;
            _catalogService = catalogService;
        }

        public async Task<CartDto> ExecuteAsync(AddToCartCommand command)
        {
            var productInfo = await _catalogService.GetProductBySkuAsync(command.Sku);
            if (productInfo == null)
            {
                throw new KeyNotFoundException($"El SKU {command.Sku} no existe en el catálogo.");
            }

            var cart = await _salesRepository.GetCartByUserIdAsync(command.UserId);
            if (cart == null)
            {
                cart = new Cart(command.UserId);
            }

            cart.AddItem(
                productId: productInfo.ProductId,
                sku: productInfo.Sku,
                productName: productInfo.ProductName,
                variantName: productInfo.VariantName,
                price: productInfo.Price,
                image: productInfo.Image
            );

            await _salesRepository.SaveCartAsync(cart);

            var itemDtos = cart.Items.Select(i => new CartItemDto(
                ProductId: i.ProductId,
                VariantId: i.VariantId,
                ProductName: i.ProductName,
                VariantName: i.VariantName,
                Price: i.Price,
                Quantity: i.Quantity,
                Image: i.Image
            )).ToList();

            return new CartDto(
                Items: itemDtos,
                TotalItems: cart.TotalItems,
                SubTotal: cart.SubTotal,
                Tax: cart.Tax,
                Total: cart.Total
            );
        }
    }
}
