namespace ProjectNative.DTOs.CartDto
{
    public class AddCartRequestDTO
    {
        public int productId { get; set; }
        public int amount { get; set; }
        public string userid { get; set; }
    }
}
