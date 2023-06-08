using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Org.BouncyCastle.Utilities;
using ProjectNative.Data;
using ProjectNative.DTOs.CartDto;
using ProjectNative.DTOs.ProductDto;
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.Services.IService;
using System.Net;
using System.Security.Claims;
using Response = ProjectNative.Models.Response;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartUserController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartUsersService _cartUsersService;

        public CartUserController(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,ICartUsersService cartUsersService)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _cartUsersService = cartUsersService;
        }

        private string GenerateID() => Guid.NewGuid().ToString("N");
        private readonly List<CartItem> Items;


        [HttpPost("[action]")]
        public async Task<IActionResult> AddItemToCart(AddCartRequestDTO addCartDTO)
        {
            var result = await _cartUsersService.AddItemToCartAsync(addCartDTO);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {

            var cart = await _dataContext.Carts.Include(a=>a.UserId).ToListAsync();

            return Ok(cart);

            //var result = _userManager.FindByNameAsync(User.Identity.Name);
            ////var cart = _dataContext.Carts.FirstOrDefaultAsync(x=>x.UserId == result.Result.Id);
            //var userName = User.FindFirstValue(ClaimTypes.Name);
            //return Ok(userName);
        }


        [HttpDelete("[action]")]

        public async Task<IActionResult> DeleteItemToCart(int productId, int amount)
        {
            var item = Items.FirstOrDefault(item => item.ProductId == productId);
            
            if (item == null)
            {
                return BadRequest();
            }
            item.Amount -= amount; //ลบสินค้าออกตามจำนวนที่ส่งมา
            if (item.Amount <= 0) Items.Remove(item); //ถ้าจำนวนสินค้ามันเป็น 0 หรือน้อยกว่า ให้ลบสินค้านั้นทิ้งไป

            await _dataContext.SaveChangesAsync();
            return Ok();
        }



    }
}
