using System.Text.Json.Serialization;

namespace ProjectNative.Models.ReviewProduct
{
    public class ReviewImage
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public int ReviewId { get; set; }
        [JsonIgnore]
        public Review Review { get; set; }
    }
}
