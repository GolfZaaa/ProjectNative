using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectNative.Models.CartAccount
{
    [Table("CartItems")]
    public class CartItem
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string CartId { get; set; }
        public Cart Cart { get; set; }
    }
}
