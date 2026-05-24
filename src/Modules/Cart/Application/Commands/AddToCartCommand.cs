using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Application.Commands
{
        public record AddToCartCommand(string CartId, string Sku, int Quantity, decimal Price);
    
}
