namespace ProjectNative.DTOs.CartDto
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; }

    }
}
