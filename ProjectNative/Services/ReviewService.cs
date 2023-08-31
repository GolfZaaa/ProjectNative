using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectNative.Data;
using ProjectNative.DTOs.ReviewDto;
using ProjectNative.Models.ReviewProduct;
using ProjectNative.Models;
using ProjectNative.Services.IService;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

namespace ProjectNative.Services
{
    public class ReviewService : ControllerBase, IReviewService
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUploadFileServiceProduct _uploadFileService;

        public ReviewService(DataContext dataContext, UserManager<ApplicationUser> userManager, IUploadFileServiceProduct uploadFileService)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _uploadFileService = uploadFileService;
        }

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


        public async Task<object> AddProductCommentAsync([FromForm] ReviewDto dto)
        {
            (string errorMessage, List<string> imageNames) = await UploadImageAsync(dto.FormFiles);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "User Not Found" });
            }
            var product = await _dataContext.Products.FirstOrDefaultAsync(a => a.Id == dto.ProductId);


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

        public async Task<object> DeleteCommentAsync(int id)
        {
            var review = await _dataContext.Reviews.FindAsync(id);

            if (review == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Review Not Found" });
            }

            _dataContext.Reviews.Remove(review);
            await _dataContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success" });

        }

        public async Task<object> DeleteImageAsync(DeleteImageDto dto)
        {
            var reviewImage = await _dataContext.ReviewImages.FindAsync(dto);

            if (reviewImage == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Image Not Found" });

            }

            _dataContext.ReviewImages.Remove(reviewImage);
            await _dataContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success" });

        }

        public async Task<object> GetProductReviewsIdAsync([FromBody] GetProductReviewsIdDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.Id == dto.UserId);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "User Not Found" });
            }
            var result = await _dataContext.Reviews
                .Where(review => review.UserId == dto.UserId)
                .Include(a => a.ReviewImages)
                .ToListAsync();
            return Ok(result);
        }

    }
}
