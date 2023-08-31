using System.ComponentModel.DataAnnotations;

namespace ProjectNative.DTOs.ProductDto
{
    public class ProductRequest
    {
        public int? Id { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage = "no more than {1} chars")]
        public string Name { get; set; }
        [Required]
        [Range(0, 1_000_000, ErrorMessage = "between {1}-{2}")]
        public long Price { get; set; }
        [Required]
        [Range(0, 1000, ErrorMessage = "between {1}-{2}")]
        public int QuantityInStock { get; set; }
        public string? Description { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public long Calorie { get; set; }
        public IFormFileCollection? FormFiles { get; set; }
        public IFormFile? FormFile { get; set; }
    }
}
