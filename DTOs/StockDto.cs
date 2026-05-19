using System.ComponentModel.DataAnnotations;

namespace InventorySystem.DTOs
{
    public class StockDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Range(1, 100000)]
        public decimal Qty { get; set; }
    }
}