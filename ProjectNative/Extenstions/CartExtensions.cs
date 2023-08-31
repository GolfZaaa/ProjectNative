using ProjectNative.DTOs.CartDto.Response;
using ProjectNative.DTOs.ProductDto.Response;
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.SettingUrl;

namespace ProjectNative.Extenstions
{
    public static class CartExtensions
    {
        public static CartResponse ToCartResponse(this Cart cart)
        {
            var cartItems = cart.Items.Select(item =>
            {
                if (item.Product != null)
                {
                    return new CartItemResponse
                    {
                        Id = item.Id,
                        Amount = item.Amount,
                        Product = FromProduct(item.Product),
                    };
                }
                else
                {
                    return null;
                }
            }).Where(item => item != null).ToList();

            return new CartResponse
            {
                Id = cart.Id,
                Items = cartItems,
                Created = cart.Created,
                UserId = cart.UserId,
                TotalPrice = cart.TotalPrice,
            };
        }



        public static ProductResponse FromProduct(Product product)
        {
            //var imageUrls = product.ProductImages.Select(a => !string.IsNullOrEmpty(a.Image) ? $"{ApplicationUrl.Url}/images/{a.Image}" : "").ToList();
            var imageUrls = product.ProductImages
                         .Where(a => !string.IsNullOrEmpty(a.Image))
                         .Select(a => a.Image)
                         .ToList();
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                Type = product.Type,
                QuantityInStock = product.QuantityInStock,
                Calorie = product.Calorie,
                Image = product.Image,
                ImageUrls = imageUrls,
            };
        }


    }
}
