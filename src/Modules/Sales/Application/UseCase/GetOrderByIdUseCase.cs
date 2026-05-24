using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.UseCase
{
    public class GetOrderByIdUseCase
    {
        private readonly ISalesRepository _salesRepository;

        public GetOrderByIdUseCase(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<OrderReceiptDto?> ExecuteAsync(string orderId)
        {
            return await _salesRepository.GetOrderByIdAsync(orderId);
        }
    }
}
