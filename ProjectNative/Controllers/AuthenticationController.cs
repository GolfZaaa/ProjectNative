using Hanssens.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ProjectNative.Data;
using ProjectNative.DTOs;
using ProjectNative.DTOs.AccConfirm;
using ProjectNative.DTOs.Address;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Models;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Services;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        LocalStorage storage = new();
        private readonly Services.TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly SendGridClient _sendGridClient;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly DataContext _dataContext;
        private readonly IUploadFileService _uploadFileService;

        public AuthenticationController(Services.TokenService tokenService, UserManager<ApplicationUser> userManager, IAccountService accountService,
            IConfiguration configuration, SendGridClient sendGridClient, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache, DataContext dataContext, IUploadFileService uploadFileService)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _accountService = accountService;
            _configuration = configuration;
            _sendGridClient = sendGridClient;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _dataContext = dataContext;
            _uploadFileService = uploadFileService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _accountService.GetUsersAsync();
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetSingleUser(UserNameDto dto)
        {
            var result = await _accountService.GetSingleUserAsync(dto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _accountService.RegisterAsync(registerDto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmail(ConfirmUserDto confirmUserDto)
        {
            var result = await _accountService.ConfirmEmailAsync(confirmUserDto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmEmail(ResendEmailconfirmDto dto)
        {
            var result = await _accountService.ResendConfirmEmailAsync(dto);
            return Ok(result);
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _accountService.LoginAsync(loginDto);

            return Ok(result);
        }



        [HttpGet("[action]")]
        public async Task<IActionResult> checkSendToEmailToken()
        {
            var data = _memoryCache.Get("Token");
            return Ok(data);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserDto dto)
        {
            var result = await _accountService.DeleteAsync(dto);
            return Ok(result);
        }





        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var result = await _accountService.ChangePasswordAsync(dto);
            return Ok(result);
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto dto)
        {
            var result = await _accountService.ForgetPasswordAsync(dto);
            return Ok(result);
        }


        [HttpPost("ChangeEmail")]
        public async Task<IActionResult> ChangeUserEmail(ChangeUserEmailDto dto)
        {
            var result = await _accountService.ChangeUserEmailAsync(dto);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeUserName(ChangeUserNameDto dto)
        {
            var result = await _accountService.ChangeUserNameAsync(dto);
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateAddress(AdressDto address)
        {
            var result = await _accountService.CreateAddressAsync(address);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> PostUserAddress([FromBody] UseridDto dto)
        {
            var result = await _accountService.PostUserAddressAsync(dto);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> SendMessageToForgotPassword(SendMessageToForgotPasswordDto dto)
        {
            var result = await _accountService.SendMessageToForgotPasswordAsync(dto);
            return Ok(result);
        }
        /// //



        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmForgotPassword(ResendConfirmForgotPasswordDto dto)
        {
            var result = await _accountService.ResendConfirmForgotPasswordAsync(dto);
            return Ok(result);
        }
        //


        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmailToForgotPassword(ConfirmForgotPasswordDto dto)
        {
            var result = await _accountService.ConfirmEmailToForgotPasswordAsync(dto);
            return Ok(result);
        }



        [HttpGet("[action]")]
        public async Task<IActionResult> CheckEmailForgotPassword()
        {
            var data = _memoryCache.Get("ForgotPassword");
            return Ok(data);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadProfileImage([FromForm] UploadProfileImageDTO dto)
        {
            var result = await _accountService.UploadProfileImageAsync(dto);
            return Ok(result);
        }
    }
}