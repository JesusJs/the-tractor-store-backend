using System;
using System.Collections.Generic;
using System.Text;
using TractorEcommerce.Modules.Catalog.Domain.Entities;

namespace TractorEcommerce.Modules.Catalog.Application.Ports
{
    public interface ICatalogRepository
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<Product?> GetByIdAsync(string id);
        Task<ProductVariant?> GetVariantBySkuAsync(string sku);
        // Para simplificar la Home (Endpoint 1), simulamos teasers basados en categorías activas
        Task<IEnumerable<string>> GetActiveCategoriesAsync();
        Task<IEnumerable<Store>> GetStoresAsync();
        Task<IEnumerable<Product>> GetProductsBySkusAsync(IEnumerable<string> skus);
        Task UpdateVariantAsync(ProductVariant variant);
    }
}
