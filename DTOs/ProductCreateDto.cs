using System.ComponentModel.DataAnnotations;

namespace Inventory.API.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string HsnCode { get; set; } = "";   // ← ADD THIS
        public List<VariantDto> Variants { get; set; }
            = new();
    }

    public class VariantDto
    {
        [Required]
        public string Name { get; set; }

        public List<string> Options { get; set; }
            = new();
    }
}