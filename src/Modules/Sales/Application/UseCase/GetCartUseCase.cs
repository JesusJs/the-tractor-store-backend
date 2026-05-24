using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.UseCase
{
    public class GetCartUseCase
    {
        private readonly ISalesRepository _salesRepository;

        public GetCartUseCase(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<CartDto> ExecuteAsync(string userId)
        {
            var cart = await _salesRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart(userId);
                await _salesRepository.SaveCartAsync(cart);
            }

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
