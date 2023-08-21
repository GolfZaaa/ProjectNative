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

        public ReviewController(DataContext dataContext, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager
            , IUploadFileServiceProduct uploadFileService)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _uploadFileService = uploadFileService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetProductReviewsId([FromBody]GetProductReviewsIdDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == dto.UserId);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "User Not Found" });
            }
            var result = await _dataContext.Reviews
                .Where(review => review.UserId == dto.UserId)
                .Include(a=>a.ReviewImages)
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductReviews()
        {
            var result = await _dataContext.Reviews.Include(a=>a.ReviewImages).ToListAsync();

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddProductComment([FromForm]ReviewDto dto)
        {
            (string errorMessage, List<string> imageNames) = await UploadImageAsync(dto.FormFiles);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "User Not Found" });
            }
            var product = await _dataContext.Products.FirstOrDefaultAsync(a=> a.Id == dto.ProductId);


            if (product == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Product Not Found" });
            }

            var Datetimenow = DateTime.Now; 
            
            var newReview = new Models.ReviewProduct.Review
            {
                Texts = dto.Texts,
                UserId = user.Id, 
                ProductId = product.Id,
                Date = Datetimenow,                
                Star = dto.Star,
                ReviewImages = imageNames.Select(imageName => new ReviewImage { Image = imageName }).ToList()
            };
            _dataContext.Reviews.Add(newReview);
            await _dataContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new ResponseReport { Status = "200", Message = "Create Review Success" });
        }

        [HttpPost]
        public async Task<(string errorMessage, List<string> imageNames)> UploadImageAsync(IFormFileCollection formFiles)
        {
            var errorMessage = string.Empty;
            var imageNames = new List<string>();
            if (_uploadFileService.IsUpload(formFiles))
            {
                errorMessage = _uploadFileService.Validation(formFiles);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    //บันทึกลงไฟล์ในโฟลเดอร์ 
                    imageNames = await _uploadFileService.UploadImages(formFiles);
                }
            }
            return (errorMessage, imageNames);
        }


    }
}
