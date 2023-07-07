using ProjectNative.DTOs.ProductDto.Response;

namespace ProjectNative.DTOs.CartDto.Response
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public ProductResponse Product { get; set; }
    }
}
