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
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Services.IService;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.Json;
using Response = ProjectNative.Models.ResponseReport;
using System.Collections;
using ProjectNative.Services;
using System.Linq;
using ProjectNative.DTOs.ProductDto.Response;
using ProjectNative.Extenstions;
using AutoMapper.Configuration.Annotations;
using ProjectNative.DTOs.OrderDto;
using Stripe;
using PaymentMethod = ProjectNative.Models.OrderAccount.PaymentMethod;

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
        private readonly IConfiguration _configuration;
        private readonly IUploadFileService _uploadFileService;

        public CartUserController(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, ICartUsersService cartUsersService, IConfiguration configuration, IUploadFileService uploadFileService)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _cartUsersService = cartUsersService;
            _configuration = configuration;
            _uploadFileService = uploadFileService;
        }

        private string GenerateID() => Guid.NewGuid().ToString("N");
        private readonly List<CartItem> Items;


        [HttpPost("[action]")]
        public async Task<IActionResult> AddItemToCart(AddCartRequestDTO addCartDTO)
        {
            var result = await _cartUsersService.AddItemToCartAsync(addCartDTO);
            return Ok(result);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> GetCart()
        {
            var carts = await _dataContext.Carts
                .Include(a => a.Items)
                    .ThenInclude(b => b.Product)
                        .ThenInclude(p => p.ProductImages)
                         .ToListAsync();

            var cartResponses = carts.Select(cart => cart.ToCartResponse()).ToList();

            return Ok(cartResponses);
        }







        [HttpPost("[action]")]
        public async Task<IActionResult> GetCartByUsername([FromBody] GetCartUserDto getCartUser)
        {
            if (getCartUser == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid request body" });
            }

            var user = await _userManager.FindByIdAsync(getCartUser.userid);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Username Not Found" });
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(a => a.ProductImages)
                .SingleOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Cart Not Found" });
            }

            cart.TotalPrice = 0;
            foreach (var item in cart.Items)
            {
                cart.TotalPrice += item.Product.Price * item.Amount;
            }

            var cartResponse = cart.ToCartResponse();

            return Ok(cartResponse);
        }






        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteItemToCart(DeleteProductInCart delete)
        {
            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == delete.UserId);

            if (cart == null)
            {
                return BadRequest();
            }

            var item = cart.Items.FirstOrDefault(item => item.ProductId == delete.ProductId);
            if (item == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "item is null" });
            }

            item.Amount -= delete.Amount; //ลบสินค้าออกตามจำนวนที่ส่งมา
            if (item.Amount <= 0)
            {
                cart.Items.Remove(item); //ถ้าจำนวนสินค้ามันเป็น 0 หรือน้อยกว่า ให้ลบสินค้านั้นทิ้งไป
            }

            await _dataContext.SaveChangesAsync();
            return Ok();
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteItemToCartall(DeleteProductInCartDtoAllcs delete)
        {
            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == delete.UserId);

            if (cart == null)
            {
                return BadRequest();
            }

            var itemsToRemove = cart.Items.Where(item => item.ProductId == delete.ProductId).ToList();

            if (itemsToRemove.Count == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "item is null" });
            }

            foreach (var item in itemsToRemove)
            {
                cart.Items.Remove(item); // ลบสินค้าออกจากตะกร้า
            }

            await _dataContext.SaveChangesAsync();
            return Ok();
        }





        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrderAsync([FromForm]OrderDto orderDTO)
        {
            var check = await _dataContext.Addresses.FirstOrDefaultAsync(a => a.UserId == orderDTO.UserId);

            if(check == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "Address not Found" });
            }


            var cart = await RetrieveCart(orderDTO.UserId);
            if (cart == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "Cart not Found" });
            }
            var user = await _dataContext.Users.SingleOrDefaultAsync(a => a.Id == orderDTO.UserId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "User not Found" });
            }

            var order = new Order
            {
                AddressId = check.Id,
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.PendingApproval,
            };

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Amount,
                    Price = item.Product.Price,
                    Order = order
                };
                order.OrderItems.Add(orderItem);
            }
            order.TotalAmount = order.GetTotalAmount();
            _dataContext.Orders.Add(order);
            await _dataContext.SaveChangesAsync();

            _dataContext.Carts.RemoveRange(cart);


            if (orderDTO.PaymentMethod == PaymentMethod.CreditCard)
            {
                var intent = await CreatePaymentIntent(order);
                if (!string.IsNullOrEmpty(intent.Id))
                {
                    order.PaymentIntentId = intent.Id; // เอาใบส่งของใส่ในใบสั่งซื้อ
                    order.ClientSecret = intent.ClientSecret; // เอารหัสลับใส่ในใบสั่งซื้อ
                };
            }
            else
            {
                (string errorMessgeMain, string imageName) =
                await UploadImageMainAsync(orderDTO.OrderImage);
                order.OrderImage = imageName;
            }
            await _dataContext.SaveChangesAsync();
            return Ok(order);
        }

        [HttpPost("[action]")]
        public async Task<(string errorMessge, string imageNames)> UploadImageMainAsync(IFormFile formfile)
        {
            var errorMessge = string.Empty;
            var imageName = string.Empty;

            if (_uploadFileService.IsUpload(formfile))
            {
                errorMessge = _uploadFileService.Validation(formfile);
                if (errorMessge is null)
                {
                    imageName = await _uploadFileService.UploadImages(formfile);
                }
            }

            return (errorMessge, imageName);
        }


        private async Task<PaymentIntent> CreatePaymentIntent(Order order)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var service = new PaymentIntentService();
            var intent = new PaymentIntent();

            //สร้างรายการใหม่
            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)order.TotalAmount * 100, // ยอดเงินเท่าไร
                    Currency = "THB", // สกุลเงิน 
                    PaymentMethodTypes = new List<string> { "card" } // วิธีการจ่าย
                };
                intent = await service.CreateAsync(options); // รหัสใบส่งของ
            };

            return intent; // ส่งใบส่งของออกไป
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
