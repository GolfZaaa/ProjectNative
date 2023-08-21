namespace ProjectNative.DTOs.ReviewDto
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public string Texts { get; set; }
        public DateTime? Date { get; set; }
        public int Star { get;set; }
        public IFormFileCollection? FormFiles { get; set; }
    }
}
