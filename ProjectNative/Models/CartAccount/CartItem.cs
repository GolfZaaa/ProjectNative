using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ProjectNative.Models.CartAccount
{
    [Table("CartItems")]
    public class CartItem
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        [JsonIgnore]
        public string CartId { get; set; }
        [JsonIgnore]
        public Cart Cart { get; set; }
    }
}
