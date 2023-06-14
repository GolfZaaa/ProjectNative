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
using ProjectNative.Models.OrderAccount;
using ProjectNative.Services.IService;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using Response = ProjectNative.Models.ResponseReport;

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


        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrderAsync(string userId)
        {
            var cart = await RetrieveCart(userId);
            if (cart == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "Cart not Found" });
            }
            var user = await _dataContext.Users.SingleOrDefaultAsync(a => a.Id == userId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "User not Found" });
            }


            var order = new Order
            {
                Id = GenerateID(),
                UserId = userId,
                OrderDate = DateTime.Now,
            };

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Amount,
                    Price = item.Product.Price,
                    OrderId = order.Id, // กำหนดค่า OrderId ให้กับ OrderItem
                };
                order.OrderItems.Add(orderItem);
            }
            order.TotalAmount = order.GetTotalAmount(); // กำหนดค่า TotalAmount
            // ทำสิ่งที่ต้องการกับ order (เช่นบันทึกลงฐานข้อมูล)
            _dataContext.Orders.Add(order);
            await _dataContext.SaveChangesAsync();
            return Ok(order); // หรือส่ง HTTP response อื่นๆ ที่เหมาะสม
        }

        private async Task<Cart> RetrieveCart(string accountId)
        {
            var cart = await _dataContext.Carts
                   .Include(i => i.Items)
                   .ThenInclude(p => p.Product)
                   .SingleOrDefaultAsync(x => x.UserId == accountId);
            return cart;
        }


    }
}
