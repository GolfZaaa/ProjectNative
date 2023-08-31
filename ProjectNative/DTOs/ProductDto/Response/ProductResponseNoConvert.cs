using ProjectNative.DTOs.ReviewDto;
using ProjectNative.Models;
using ProjectNative.SettingUrl;

namespace ProjectNative.DTOs.ProductDto.Response
{
    public class ProductResponseNoConvert
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public int QuantityInStock { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public long Calorie { get; set; }
        public string? Image { get; set; }

        public List<string> ImageUrls { get; set; }
        public List<ReviewResponse> Reviews { get; set; }


        static public ProductResponseNoConvert FromProductNoConvert(Product product)
        {
            var reviewResponses = product.Reviews.Select(review => ReviewResponse.FromReview(review)).ToList();




            return new ProductResponseNoConvert
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Type = product.Type,
                QuantityInStock = product.QuantityInStock,
                Calorie = product.Calorie,
                ImageUrls = product.ProductImages.Select(image => image.Image).ToList(),
                Image = product.Image,
                Reviews = reviewResponses,
            };
        }
    }
}
