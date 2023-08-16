using ProjectNative.Data;
using ProjectNative.DTOs.ProductDto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectNative.Models.OrderAccount
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        //public string UserId { get; set; }
        //[ForeignKey("UserId")]
        //public ApplicationUser User { get; set; }
        
        public string? OrderImage { get; set; }
        public DateTime OrderDate { get; set; }
        public long TotalAmount { get; set; }
        [JsonIgnore]
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public string? PaymentIntentId { get; set; }
        public string? ClientSecret { get; set; } 

        public int AddressId { get; set; }
        public Address Address { get; set; }

        public OrderStatus OrderStatus { get; set; }





        public long GetTotalAmount()
        {
            long total = 0;
            foreach (var item in OrderItems)
            {
                total += item.Quantity * item.Price;
            }
            return total;
        }
    }
}
