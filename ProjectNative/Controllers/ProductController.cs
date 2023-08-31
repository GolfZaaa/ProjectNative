using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.ProductDto;
using ProjectNative.DTOs.ProductDto.Response;
using ProjectNative.DTOs.ReviewDto;
using ProjectNative.Models;
using ProjectNative.Models.ReviewProduct;
using ProjectNative.Services;
using ProjectNative.Services.IService;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly DataContext _dataContext;

        public ProductController(IProductService productService, DataContext dataContext)
        {
            _productService = productService;
            _dataContext = dataContext;
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Get()
        {
            var result = await _productService.GetProductListAsync();
            var response = result.Select(ProductResponse.FromProduct).ToList();
            return Ok(response);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetProductNoConvert()
        {
            var result = await _productService.GetProductListAsync();
            var response = result.Select(ProductResponseNoConvert.FromProductNoConvert).ToList();
            return Ok(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddProduct([FromForm] ProductRequest request)
        {
            var result = await _productService.CreateAsync(request);
            if (result != null) return BadRequest(result);
            return Ok();
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTypes()
        {
            var result = await _productService.GetTypeAsync();
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductRequest productRequest)
        {
            var result = await _productService.GetByIdAsync((int)productRequest.Id);
            if (result == null) return NotFound();
            var resultUpdate = await _productService.UpdateAsync(productRequest);
            if (resultUpdate != null) return BadRequest(resultUpdate);
            return Ok(result);
        }



        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (result == null) return NotFound();
            await _productService.DeleteAsync(result);
            return Ok (new { status = "Deleted", result });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> SearchProduct([FromQuery] string name = "")
        {
            var result = (await _productService.SearchAsync(name))
                .Select(ProductResponse.FromProduct).ToList();
            return Ok(result);
        }


        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(ProductResponse.FromProduct(result));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages()
        {
            var reviewImages = await _dataContext.ProductImages.ToListAsync();
            return Ok(reviewImages);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteImagesProduct(DeleteImageDto dto)
        {
            var result = await _dataContext.ProductImages.FindAsync(dto.Id);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Image Not Found" });
            }

            _dataContext.ProductImages.Remove(result);
            await _dataContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "201", Message = "Delete Image Success" });
        }


    }
}
