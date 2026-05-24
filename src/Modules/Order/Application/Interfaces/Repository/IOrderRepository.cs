using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Domain.Entities;

namespace TractorEcommerce.Modules.Order.Application.Interfaces.Repository
{
    public interface IOrderRepository
    {
        Task SaveAsync(CustomerOrder order);
    }
}
