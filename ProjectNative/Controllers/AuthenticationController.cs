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
using ProjectNative.Models;
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

        public AuthenticationController(Services.TokenService tokenService, UserManager<ApplicationUser> userManager, IAccountService accountService,
            IConfiguration configuration, SendGridClient sendGridClient, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache, DataContext dataContext)
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

        //private async Task SendEmailConfirmationEmail(string email, string token)
        //{
        //    var cachedToken = _memoryCache.Get<string>("Token");
        //    if (!string.IsNullOrEmpty(cachedToken))
        //    {
        //        // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
        //        var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
        //        var to = new EmailAddress(email);
        //        var subject = "Thank you";

        //        var htmlContent = $"<p>Thank you for registering! Please confirm your email address by using the following token:</p>";
        //        htmlContent += $"<p><strong>{cachedToken}</strong></p>";

        //        var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
        //        await _sendGridClient.SendEmailAsync(emailMessage);

        //        ConfirmUserDto confirmUserDto = new()
        //        {
        //            Email = email,
        //            EmailConfirmationToken = cachedToken
        //        };

        //    }
        //}

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
        public async Task<IActionResult> ForgetPassword (ForgetPasswordDto dto)
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
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid user." });
            }

            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            string tokenforgotpassword = randomNumber.ToString();
            _memoryCache.Set("ForgotPassword", tokenforgotpassword, TimeSpan.FromDays(1));

            if (!string.IsNullOrEmpty(tokenforgotpassword))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailForgotPassword(dto.Email, tokenforgotpassword);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "ไม่ได้รับค่า SendEmailForgotPassword ที่ถูกต้อง" });
            }
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success to SendEmail for Forgotpassword" });
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ResendConfirmForgotPassword(ResendConfirmForgotPasswordDto dto)
        {
            _memoryCache.Remove("ForgotPassword");
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            string tokenforgotpassword = randomNumber.ToString();
            _memoryCache.Set("ForgotPassword", tokenforgotpassword, TimeSpan.FromDays(1));

            if (!string.IsNullOrEmpty(tokenforgotpassword))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailForgotPassword(dto.Email, tokenforgotpassword);
            }
            else
            {
                // กรณีไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "ไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง" });
            }
            return StatusCode(StatusCodes.Status202Accepted);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmailToForgotPassword(ConfirmForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid user." });
            }

            var token = _memoryCache.Get<string>("ForgotPassword"); // รับค่า token จาก memory cache

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "token null." });
            }

            if (string.IsNullOrEmpty(dto.ConfirmForgotPassowrd))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "EmailConfirmationToken null ." });
            }

            // เช็คว่าโทเค็นที่ผู้ใช้กรอกตรงกับโทเค็นที่เก็บในแคชไหม
            if (dto.ConfirmForgotPassowrd != token)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid email confirmation token." });
            }
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Confirmed token successfully go to ResetPassword." });
        }

        private async Task SendEmailForgotPassword(string email, string token)
        {
            var cachedTokenforgotpassword = _memoryCache.Get<string>("ForgotPassword");
            if (!string.IsNullOrEmpty(cachedTokenforgotpassword))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
                var to = new EmailAddress(email);
                var subject = "ForgotPassword";

                var htmlContent = "<div style=\"text-align: center;\">";
                htmlContent += "<p><strong><h1 style=\"font-size:2em; \">Forgot Password</h1></strong></p>";
                htmlContent += "<p>Enter the email address you used when you joined and we’ll send you instructions to reset your password.</p>";
                htmlContent += $"<p><strong><h1 style=\"font-size:4em; \">{cachedTokenforgotpassword}</h1></strong></p>";
                htmlContent += "</div>";

                var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
                await _sendGridClient.SendEmailAsync(emailMessage);

                ConfirmForgotPasswordDto dto = new()
                {
                    Email = email,
                    ConfirmForgotPassowrd = cachedTokenforgotpassword
                };
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> CheckEmailForgotPassword()
        {
            var data = _memoryCache.Get("ForgotPassword");
            return Ok(data);
        }



    }
}
