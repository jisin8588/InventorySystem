namespace Inventory.API.Models
{
    public class StockTransaction
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public decimal Quantity { get; set; }

        public string TransactionType { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}