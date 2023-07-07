using ProjectNative.Models.CartAccount;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjectNative.Models.OrderAccount
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; } // เพิ่มคีย์หลักให้กับ OrderItem
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; } 
        public long Price { get; set; }
        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }
    }
}
