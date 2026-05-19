using Inventory.API.Data;
using Inventory.API.DTOs;
using Inventory.API.Models;
//using Inventory.API.Services;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsAsync(int page, int pageSize)
        {
            return await _context.Products
                .Include(x => x.Variants)
                .ThenInclude(x => x.Options)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> AddStockAsync(Guid productId, decimal qty)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                throw new Exception("Product not found");

            product.TotalStock += qty;

            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = qty,
                TransactionType = "PURCHASE",
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> RemoveStockAsync(Guid productId, decimal qty)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                throw new Exception("Product not found");

            if (product.TotalStock < qty)
                throw new Exception("Insufficient stock");

            product.TotalStock -= qty;

            _context.StockTransactions.Add(new StockTransaction
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = qty,
                TransactionType = "SALE",
                CreatedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return product;
        }
    }
}