using ProjectNative.DTOs.AccountUser.Response;
using ProjectNative.Models;
using ProjectNative.Models.ReviewProduct;
using ProjectNative.SettingUrl;

namespace ProjectNative.DTOs.ReviewDto
{
    public class ReviewResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserResponse User { get; set; }

        public string Texts { get; set; }
        public DateTime Date { get; set; }
        public int Star { get; set; }
        public List<string> ReviewImageUrls { get; set; }
        public List<string> ProfileUser { get; set; }
        public static ReviewResponse FromReview(Review review)
        {
            var reviewImageUrls = review.ReviewImages.Select(img => !string.IsNullOrEmpty(img.Image) ? $"{ApplicationUrl.Url}/images/{img.Image}" : "").ToList();


            var ProfileUser = new List<string>();
            if (review.User != null && !string.IsNullOrEmpty(review.User.ProfileImage))
            {
                ProfileUser.Add($"{ApplicationUrl.Url}/orderImage/{review.User.ProfileImage}");
            }


            return new ReviewResponse
            {
                Id = review.Id,
                UserId = review.UserId,
                User = UserResponse.FromUser(review.User),
                Texts = review.Texts,
                Date = review.Date,
                Star = review.Star,
                ReviewImageUrls = reviewImageUrls,
                ProfileUser = ProfileUser,
            };
        }
    }
}
