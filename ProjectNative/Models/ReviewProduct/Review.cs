using ProjectNative.Models.OrderAccount;

namespace ProjectNative.Models.ReviewProduct
{
    public class Review
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Texts { get; set; }
        public DateTime Date { get; set; }
        public int Star { get;set; }
        public List<ReviewImage> ReviewImages { get; set; }
        public int OrderItemId { get; set; }
    }
}
