using ProjectNative.Models;
using ProjectNative.SettingUrl;

namespace ProjectNative.DTOs.ProductDto.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public int QuantityInStock { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public long Calorie { get; set; }

        public List<string> ImageUrls { get; set; }


        static public ProductResponse FromProduct(Product product)
        {
            var imageUrls = product.ProductImages.Select(a => !string.IsNullOrEmpty(a.Image) ? $"{ApplicationUrl.Url}/images/{a.Image}" : "").ToList();
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Type = product.Type,
                QuantityInStock = product.QuantityInStock,
                Calorie = product.Calorie,
                ImageUrls = imageUrls
            };
        }




    }
}
