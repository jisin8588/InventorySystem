using Inventory.API.Models;

namespace InventorySystem.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync(int page, int pageSize);

        Task<Product> AddStockAsync(Guid productId, decimal qty);

        Task<Product> RemoveStockAsync(Guid productId, decimal qty);
    }
}