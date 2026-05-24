using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Order.Application.DTOs;
using TractorEcommerce.Modules.Order.Application.Interfaces.Repository;
using TractorEcommerce.Modules.Order.Domain.Entities;
using TractorEcommerce.Modules.Order.Infrastructure.Data;

namespace TractorEcommerce.Modules.Order.Infrastructure.Repository
{
public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        // SÓLO CORRIGE ESTA LÍNEA (quitando la palabra 'class'):
        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrderAsync(OrderReceiptDto order)
        {
            // 1. Resolver el Guid de la Orden de forma segura
            if (!Guid.TryParse(order.Id, out var orderGuid))
            {
                orderGuid = Guid.NewGuid();
            }

            // 2. Mapear los ítems del DTO a tus entidades hijas 'OrderLineItem'
            // (Ajusta el constructor de tu OrderLineItem si difiere en nombres)
            var lineItems = order.Items.Select(item => new OrderLineItem(
                Guid.TryParse(item.VariantId, out var vGuid) ? vGuid : Guid.NewGuid(),
                item.ProductName,
                item.Quantity,
                item.Price
            )).ToList();

            // 3. Instanciar tu Entidad usando SU VERDADERO CONSTRUCTOR
            // Como tu DTO no trae explícitamente un 'CustomerId' de sesión, concatenamos o usamos un identificador único
            string customerIdentifier = $"{order.FirstName}_{order.LastName}".Replace(" ", "");

            var customerOrder = new CustomerOrder(
                id: orderGuid,
                customerId: customerIdentifier,
                items: lineItems
            );

            // 4. Guardar limpiamente en la DB vía EF Core
            await _context.Orders.AddAsync(customerOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<OrderReceiptDto?> GetOrderByIdAsync(string orderId)
        {
            // 1. Validar si el string que viene del Front/Caso de uso es un Guid válido
            if (!Guid.TryParse(orderId, out var guidId))
            {
                return null; 
            }

            // 2. Traer la orden INCLUYENDO sus ítems hijos (Crucial para no dejar la lista vacía)
            var entity = await _context.Orders
                .Include(o => o.Items) 
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == guidId);

            if (entity == null) return null;

            // 3. Mapear de regreso al DTO que exige tu capa de Aplicación
            var itemDetails = entity.Items.Select(i => new OrderItemDetailDto(
                ProductId: string.Empty, // Completa si tu OrderLineItem guarda el ProductId
                VariantId: i.Id.ToString(),
                ProductName: i.ProductName, // O la propiedad que mapee el nombre en tu OrderLineItem
                VariantName: null,
                Price: i.UnitPrice,
                Quantity: i.Quantity,
                Image: null
            )).ToList();

            // Desglosamos IVA basándonos en tu 'TotalAmount' de dominio
            decimal subTotal = entity.TotalAmount / 1.19m;
            decimal tax = entity.TotalAmount - subTotal;

            return new OrderReceiptDto(
                Id: entity.Id.ToString(),
                FirstName: entity.CustomerId, // O recuperas el split si lo necesitas exacto
                LastName: string.Empty,
                StoreId: "STORE_PICKUP", 
                ExtraPickups: null,
                Items: itemDetails,
                SubTotal: Math.Round(subTotal, 2),
                Tax: Math.Round(tax, 2),
                Total: entity.TotalAmount,
                PlacedAt: entity.CreatedAt,
                Status: "Pending"
            );
        }
    }
}
