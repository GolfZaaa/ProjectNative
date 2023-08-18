using ProjectNative.Models.OrderAccount;

namespace ProjectNative.DTOs.OrderDto
{
    public class OrderDto
    {
        public string UserId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public IFormFile? OrderImage { get; set; }
    }
}
