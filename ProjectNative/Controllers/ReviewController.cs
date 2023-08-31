using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.ReviewDto;
using ProjectNative.Models;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Models.ReviewProduct;
using ProjectNative.Services.IService;
using SendGrid.Helpers.Mail;
using Stripe;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUploadFileServiceProduct _uploadFileService;
        private readonly IReviewService _reviewService;

        public ReviewController(DataContext dataContext, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager
            , IUploadFileServiceProduct uploadFileService, IReviewService reviewService)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _uploadFileService = uploadFileService;
            _reviewService = reviewService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetProductReviewsId([FromBody] GetProductReviewsIdDto dto)
        {
            var result = await _reviewService.GetProductReviewsIdAsync(dto);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductReviews()
        {
            var result = await _dataContext.Reviews.Include(a => a.ReviewImages).ToListAsync();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _reviewService.DeleteCommentAsync(id);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteImage(DeleteImageDto dto)
        {
            var result = await _reviewService.DeleteImageAsync(dto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddProductComment([FromForm] ReviewDto dto)
        {
            var result = await _reviewService.AddProductCommentAsync(dto);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ReviewImage>>> GetReviewImages()
        {
            var reviewImages = await _dataContext.ReviewImages.ToListAsync();
            return Ok(reviewImages);
        }

    }
}