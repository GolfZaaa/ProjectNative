using ProjectNative.Models.ReviewProduct;
using System.Text.Json.Serialization;

namespace ProjectNative.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long Price { get; set; }
        public int QuantityInStock { get; set; }
        public string? Description { get; set; }
        public long Calorie { get; set; }

        [JsonIgnore]
        public List<ProductImage> ProductImages { get; set; }
        [JsonIgnore]
        public List<Review> Reviews { get; set; }
    }
}
