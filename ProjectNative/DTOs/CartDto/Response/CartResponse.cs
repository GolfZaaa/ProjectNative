namespace ProjectNative.DTOs.CartDto.Response
{
    public class CartResponse
    {
        public string Id { get; set; }
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        public DateTime Created { get; set; }
        public string UserId { get; set; }

        public decimal TotalPrice { get; set; }

    }
}
