using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.UseCase
{
    public class GetMiniCartUseCase
    {
        private readonly ISalesRepository _salesRepository;

        public GetMiniCartUseCase(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<MiniCartDto> ExecuteAsync(string userId)
        {
            var cart = await _salesRepository.GetCartByUserIdAsync(userId);
            return new MiniCartDto(cart?.TotalItems ?? 0);
        }
    }
}
