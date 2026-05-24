using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.Interfaces.Repository
{
    public interface ISalesRepository
    {
        Task<Cart?> GetCartByUserIdAsync(string userId);
        Task SaveCartAsync(Cart Cart);
        Task SaveOrderReceiptAsync(OrderReceiptDto order);
        Task<OrderReceiptDto?> GetOrderByIdAsync(string id);
        Task<IDbTransactionWrapper> BeginTransactionAsync();
    }
}
