using System;

namespace TractorEcommerce.Modules.Sales.Domain.Entities
{
    public class OrderReceiptItem
    {
        public string Sku { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public string VariantName { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public string Image { get; private set; }

        private OrderReceiptItem() { }

        public OrderReceiptItem(string sku, string productId, string productName, string variantName, decimal price, int quantity, string image)
        {
            Sku = sku;
            ProductId = productId;
            ProductName = productName;
            VariantName = variantName;
            Price = price;
            Quantity = quantity;
            Image = image;
        }
    }
}
