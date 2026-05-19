using Inventory.API.Data;
using Inventory.API.DTOs;
using Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Product name required");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                ProductName = dto.Name,
                ProductCode = "PRD-" + DateTime.Now.Ticks,
                CreatedDate = DateTimeOffset.UtcNow,
                UpdatedDate = DateTimeOffset.UtcNow,
                Active = true,
                HSNCode = dto.HsnCode ?? "", 
                TotalStock = 0,
                Variants = new List<ProductVariant>()
            };

            foreach (var variantDto in dto.Variants)
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    Name = variantDto.Name,
                    Options = new List<VariantOption>()
                };

                foreach (var option in variantDto.Options)
                {
                    variant.Options.Add(new VariantOption
                    {
                        Id = Guid.NewGuid(),
                        OptionValue = option
                    });
                }

                product.Variants.Add(variant);
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            int page = 1,
            int pageSize = 10)
        {
            var products = await _context.Products
                .Include(x => x.Variants)
                .ThenInclude(x => x.Options)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(products);
        }


        [HttpPost("add-stock")]
        public async Task<IActionResult> AddStock(Guid productId, decimal qty)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

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

            return Ok(product);
        }
        [HttpPost("remove-stock")]
        public async Task<IActionResult> RemoveStock(Guid productId, decimal qty)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound();

            if (product.TotalStock < qty)
                return BadRequest("Insufficient stock");

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

            return Ok(product);
        }

    }
}