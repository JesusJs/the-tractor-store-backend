using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Cart.Infrastructure.Messaging
{
    public record OrderPlacedEventData(
      string OrderId,
      string UserId
  );
}
