using Microsoft.AspNetCore.Mvc;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.DTOs.ReviewDto;

namespace ProjectNative.Services.IService
{
    public interface IReviewService
    {
        Task<Object> DeleteCommentAsync(int id);
        Task<Object> DeleteImageAsync(DeleteImageDto dto);

        Task<Object> AddProductCommentAsync([FromForm] ReviewDto dto);
        Task<Object> GetProductReviewsIdAsync([FromBody] GetProductReviewsIdDto dto);


    }
}
