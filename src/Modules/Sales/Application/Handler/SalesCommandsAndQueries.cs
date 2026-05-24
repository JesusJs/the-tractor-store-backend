using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TractorEcommerce.Modules.Sales.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Sales.Domain.Entities;
using static TractorEcommerce.Modules.Sales.Application.DTOs.SalesDtos;

namespace TractorEcommerce.Modules.Sales.Application.Handler
{
    public record AddToCartCommand(
        string UserId,
        string ProductId,
        string Sku,
        string ProductName,
        string VariantName,
        decimal Price,
        string Image
    );

    public class AddToCartCommandHandler
    {
        private readonly ISalesRepository _salesRepository;

        public AddToCartCommandHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<CartDto> ExecuteAsync(AddToCartCommand command)
        {
            var cart = await _salesRepository.GetCartByUserIdAsync(command.UserId);
            if (cart == null)
            {
                cart = new Cart(command.UserId);
            }

            cart.AddItem(
                productId: command.ProductId,
                sku: command.Sku,
                productName: command.ProductName,
                variantName: command.VariantName,
                price: command.Price,
                image: command.Image
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

    public class RemoveFromCartCommandHandler
    {
        private readonly ISalesRepository _salesRepository;

        public RemoveFromCartCommandHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<CartDto> ExecuteAsync(string userId, string sku)
        {
            var cart = await _salesRepository.GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                cart.RemoveItem(sku);
                await _salesRepository.SaveCartAsync(cart);
            }
            else
            {
                cart = new Cart(userId);
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

    public class GetCartQueryHandler
    {
        private readonly ISalesRepository _salesRepository;

        public GetCartQueryHandler(ISalesRepository salesRepository)
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

    public class GetMiniCartQueryHandler
    {
        private readonly ISalesRepository _salesRepository;

        public GetMiniCartQueryHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<MiniCartDto> ExecuteAsync(string userId)
        {
            var cart = await _salesRepository.GetCartByUserIdAsync(userId);
            return new MiniCartDto(cart?.TotalItems ?? 0);
        }
    }

    public class GetOrderByIdQueryHandler
    {
        private readonly ISalesRepository _salesRepository;

        public GetOrderByIdQueryHandler(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public async Task<OrderReceiptDto?> ExecuteAsync(string orderId)
        {
            return await _salesRepository.GetOrderByIdAsync(orderId);
        }
    }
}
