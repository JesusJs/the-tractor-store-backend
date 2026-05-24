using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Application.DTOs
{
    public class SalesDtos
    {
        // ==========================================
        // RESPUESTAS DEL CARRITO (Endpoints 7, 9 y 10)
        // ==========================================

        /// <summary>
        /// Representa el objeto de carrito completo con los cálculos e IVA (21%) ya procesados.
        /// </summary>
        public record CartDto(
            IEnumerable<CartItemDto> Items,
            int TotalItems,
            decimal SubTotal,
            decimal Tax,
            decimal Total
        );

        /// <summary>
        /// Detalle individual de cada tractor o variante dentro del carrito.
        /// </summary>
        public record CartItemDto(
            string ProductId,
            string VariantId, // Mapea directamente al SKU del Frontend
            string ProductName,
            string VariantName,
            decimal Price,
            int Quantity,
            string Image
        );

        // ==========================================
        // MINI CART (Endpoint 8)
        // ==========================================

        /// <summary>
        /// Estructura ultra liviana para el Header (Shell) que solo indica cantidad de piezas.
        /// </summary>
        public record MiniCartDto(
            int Quantity
        );

        // ==========================================
        // SOLICITUDES / REQUEST BODIES (Endpoints 9 y 11)
        // ==========================================

        /// <summary>
        /// Payload recibido al agregar un producto al carrito.
        /// </summary>
        public record AddToCartRequest(
            string Sku
        );

        /// <summary>
        /// Datos de facturación, envío y recogida enviados por el Checkout al procesar la compra.
        /// </summary>
        public record OrderPayloadDto(
            string FirstName,
            string LastName,
            string StoreId,
            IEnumerable<string> ExtraPickups
        );

        // ==========================================
        // RESPUESTAS DE ÓRDENES (Endpoints 11 y 12)
        // ==========================================

        /// <summary>
        /// El recibo oficial generado tras consolidar la orden de compra con éxito.
        /// </summary>
        public record OrderReceiptDto(
            string Id, // Ej: ORD-123456
            string FirstName,
            string LastName,
            string StoreId,
            IEnumerable<string> ExtraPickups,
            IEnumerable<CartItemDto> Items,
            decimal SubTotal,
            decimal Tax,
            decimal Total,
            DateTime PlacedAt // Se enviará en formato ISO UTC
        );
    }
}
