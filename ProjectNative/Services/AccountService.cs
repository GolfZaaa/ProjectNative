using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using ProjectNative.DTOs;
using ProjectNative.DTOs.AccConfirm;
using ProjectNative.Models;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ProjectNative.Services
{
    public class AccountService : ControllerBase, IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SendGridClient _sendGridClient;
        private readonly IMemoryCache _memoryCache;

        public AccountService(UserManager<ApplicationUser> userManager, TokenService tokenService, IHttpContextAccessor httpContextAccessor, RoleManager<IdentityRole> roleManager,
            SendGridClient sendGridClient, IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _sendGridClient = sendGridClient;
            _memoryCache = memoryCache;
        }

       

        public async Task<object> ConfirmEmailAsync(ConfirmUserDto confirmUserDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmUserDto.Email);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid user." });
            }

            var token = _memoryCache.Get<string>("Token"); // รับค่า token จาก memory cache

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "token null." });
            }

            if (string.IsNullOrEmpty(confirmUserDto.EmailConfirmationToken))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "EmailConfirmationToken null ." });
            }

            // เช็คว่าโทเค็นที่ผู้ใช้กรอกตรงกับโทเค็นที่เก็บในแคชไหม
            if (confirmUserDto.EmailConfirmationToken != token)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid email confirmation token." });
            }

            // อัปเดตสถานะการยืนยันอีเมล์ในฐานข้อมูล
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Email confirmed successfully." });
        }

        public async Task<object> DeleteAsync(string username)
        {
            var check = await _userManager.FindByNameAsync(username);

            if (check != null)
            {
                await _userManager.DeleteAsync(check);
                return Ok(new ResponseReport { Status = "201", Message = "Deleted Successfully" });
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Failed to Delete" });
            }
        }

        public object GetMe()
        {
            var username = string.Empty;
            var role = string.Empty;


            if (_httpContextAccessor.HttpContext != null)
            {
                username = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
                role = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            }
            return new { username, role };
        }



        public async Task<object> GetSingleUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Username Not Found" });
            }

            var userRole = await _userManager.GetRolesAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var securitystamp = await _userManager.GetSecurityStampAsync(user);
            var userid = await _userManager.GetUserIdAsync(user);
            var name = await _userManager.GetUserNameAsync(user);

            var userDetails = new { name, userRole, email, securitystamp, userid };

            return userDetails;
        }


        public async Task<List<object>> GetUsersAsync()
        {
            var result = await _userManager.Users.ToListAsync();
            List<Object> users = new();
            foreach (var user in result)
            {
                var userRole = await _userManager.GetRolesAsync(user);
                users.Add(new { user.UserName, userRole });
            }
            return (users);
        }

        public async Task<object> LoginAsync(LoginDto loginDto)
        {
            var check = await _userManager.FindByNameAsync(loginDto.Username);

            if (check == null || !await _userManager.CheckPasswordAsync(check, loginDto.Password))
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Invalid username or password. Please try again." });
            }

            if (check.EmailConfirmed == false)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "404", Message = "Please confirm your email for the first login." });
            }
            else
            {
                var userDto = new UserDto
                {
                    Email = check.Email,
                    Token = await _tokenService.GenerateToken(check),
                };
                return (userDto);
            }

            //var userDto = new UserDto
            //{
            //    Email = check.Email,
            //    Token = await _tokenService.GenerateToken(check),
            //};
            //return (userDto);

        }

        public async Task<object> RegisterAsync(RegisterDto registerDto)
        {
            var check = await _userManager.FindByEmailAsync(registerDto.Email);
            if (check != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "404", Message = "This e-mail has already been used." });
            }
            var roleExists = await _roleManager.RoleExistsAsync(registerDto.Role);
            if (!roleExists)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "404", Message = "The specified role does not exist." });
            }
            var createuser = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = false,
            };
            var result = await _userManager.CreateAsync(createuser, registerDto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }
            await _userManager.AddToRoleAsync(createuser, registerDto.Role);
            // สร้าง token สำหรับการยืนยันอีเมล์
            var token = Guid.NewGuid().ToString();
            _memoryCache.Set("Token", token, TimeSpan.FromDays(1));

            check.EmailConfirmed = false;

            await _userManager.UpdateAsync(createuser);

            if (!string.IsNullOrEmpty(token))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailConfirmationEmail(createuser.Email, token);
            }
            else
            {
                // กรณีไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "ไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง" });
            }
            return StatusCode(StatusCodes.Status201Created, new ResponseReport { Status = "201", Message = "Create Successfully" });
        }

        private async Task SendEmailConfirmationEmail(string email, string token)
        {
            var cachedToken = _memoryCache.Get<string>("Token");
            if (!string.IsNullOrEmpty(cachedToken))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
                var to = new EmailAddress(email);
                var subject = "Thank you";

                var htmlContent = $"<p>Thank you for registering! Please confirm your email address by using the following token:</p>";
                htmlContent += $"<p><strong>{cachedToken}</strong></p>";

                var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
                await _sendGridClient.SendEmailAsync(emailMessage);

                ConfirmUserDto confirmUserDto = new()
                {
                    Email = email,
                    EmailConfirmationToken = cachedToken
                };

                await ConfirmEmailAsync(confirmUserDto);
            }
        }

    }
}
