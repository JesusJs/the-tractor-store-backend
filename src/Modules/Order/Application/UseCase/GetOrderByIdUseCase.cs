using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;

namespace TractorEcommerce.Modules.Order.Application.UseCase
{
    public class GetOrderByIdUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderReceiptDto?> ExecuteAsync(string orderId)
        {
            return await _orderRepository.GetOrderByIdAsync(orderId);
        }
    }
}
