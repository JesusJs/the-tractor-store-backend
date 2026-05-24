using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Catalog.Application.DTOs
{
    public record StoreResponse(
     string Id,
     string Name,
     string Address,
     string City,
     string Image
 );
}
