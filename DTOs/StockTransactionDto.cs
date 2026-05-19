using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs
{
    public class StockTransactionDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 999999)]
        public decimal Quantity { get; set; }
    }
}