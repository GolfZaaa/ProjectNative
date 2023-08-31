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
using SendGrid.Helpers.Mail;
using SendGrid;

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
        private readonly SendGridClient _sendGridClient;

        public CartUserController(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, ICartUsersService cartUsersService, IConfiguration configuration, IUploadFileService uploadFileService, SendGridClient sendGridClient)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _cartUsersService = cartUsersService;
            _configuration = configuration;
            _uploadFileService = uploadFileService;
            _sendGridClient = sendGridClient;
        }

        private string GenerateID() => Guid.NewGuid().ToString("N");
        private readonly List<CartItem> Items;


        [HttpPost("[action]")]
        public async Task<IActionResult> AddItemToCart(AddCartRequestDTO addCartDTO)
        {
            var result = await _cartUsersService.AddItemToCartAsync(addCartDTO);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> GetCartByUsername([FromBody] GetCartUserDto getCartUser)
        {
            var result = await _cartUsersService.GetCartByUsernameAsync(getCartUser);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteItemToCart(DeleteProductInCart delete)
        {
            var result = await _cartUsersService.DeleteItemToCartAsync(delete);
            return Ok(result);
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteItemToCartall(DeleteProductInCartDtoAllcs delete)
        {
            var result = await _cartUsersService.DeleteItemToCartallAsync(delete);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateOrderAsync([FromForm] OrderDto orderDTO)
        {
            var result = await _cartUsersService.CreateOrderAsyncReal(orderDTO);
            return Ok(result);
        }

    }
}