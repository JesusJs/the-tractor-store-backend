using System;
using System.Collections.Generic;
using System.Text;

namespace TractorEcommerce.Modules.Sales.Application.Interfaces
{
    public interface IDbTransactionWrapper
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
