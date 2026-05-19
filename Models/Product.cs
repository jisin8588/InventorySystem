using System.ComponentModel.DataAnnotations;

namespace Inventory.API.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; }

        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset UpdatedDate { get; set; }

        public Guid CreatedUser { get; set; }

        public bool IsFavourite { get; set; }

        public bool Active { get; set; }

        [MaxLength(100)]
        public string HSNCode { get; set; }

        public decimal TotalStock { get; set; }

        public ICollection<ProductVariant> Variants { get; set; }
            = new List<ProductVariant>();

        public ICollection<StockTransaction> StockTransactions { get; set; }
            = new List<StockTransaction>();
    }
}