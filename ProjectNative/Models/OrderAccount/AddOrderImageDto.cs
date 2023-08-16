namespace ProjectNative.Models.OrderAccount
{
    public class AddOrderImageDto
    {
        public int orderId { get; set; }
        public IFormFile? orderImage { get; set; }
    }
}
