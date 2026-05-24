using System.Threading.Tasks;

namespace TractorEcommerce.Modules.Sales.Application.Interfaces.Service
{
    public record CatalogProductInfo(
        string ProductId,
        string Sku,
        string ProductName,
        string VariantName,
        decimal Price,
        string Image
    );

    public interface ICatalogService
    {
        Task<CatalogProductInfo?> GetProductBySkuAsync(string sku);
    }
}
