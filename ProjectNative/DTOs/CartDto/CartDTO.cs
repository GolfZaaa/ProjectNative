namespace ProjectNative.DTOs.CartDto
{
    public class CartDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<CartItemDTO>? Items { get; set; }
    }
}
