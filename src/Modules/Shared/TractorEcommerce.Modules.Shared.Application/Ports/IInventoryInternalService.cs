namespace TractorEcommerce.Modules.Shared.TractorEcommerce.Modules.Shared.Application.Ports
{
    public interface IInventoryInternalService
    {
        // Permite que otros módulos consulten el stock actual de forma síncrona y eficiente
        Task<int> GetStockBySkuAsync(string sku);
        Task<bool> HasEnoughStockAsync(string sku, int quantity);
    }
}
