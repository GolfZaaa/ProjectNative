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
using ProjectNative.Data;
using ProjectNative.DTOs;
using ProjectNative.DTOs.AccConfirm;
using ProjectNative.DTOs.Address;
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
        private readonly DataContext _dataContext;

        public AccountService(UserManager<ApplicationUser> userManager, TokenService tokenService, IHttpContextAccessor httpContextAccessor, RoleManager<IdentityRole> roleManager,
            SendGridClient sendGridClient, IMemoryCache memoryCache, DataContext dataContext)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _sendGridClient = sendGridClient;
            _memoryCache = memoryCache;
            _dataContext = dataContext;
        }

        public async Task<object> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return Unauthorized();
            }

            var changePasswordResult = await _userManager.RemovePasswordAsync(user);

            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to change password" });
            }

            changePasswordResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to change password" });
            }
            return Ok(new { Message = "Password changed successfully" });
        }

        public async Task<object> ChangeUserEmailAsync(ChangeUserEmailDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            if (user.Email == dto.NewEmail)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "400", Message = "The new Email is the same as the current Email you are using. Please enter a Email that is different from the current one." });
            }
            var result = await _userManager.SetEmailAsync(user, dto.NewEmail);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Fail to SetEmail" });
            }

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Change Email Successfuly" });
        }

        public async Task<object> ChangeUserNameAsync(ChangeUserNameDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }


            if (user.UserName == dto.NewUserName)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "400", Message = "The new UserName is the same as the current UserName you are using. Please enter a UserName that is different from the current one." });
            }

            var result = await _userManager.SetUserNameAsync(user, dto.NewUserName);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Fail to SetUserName" });
            }
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Change UserName Successfully, please logout and login to get token again" });
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
            else
            {
                user.EmailConfirmed = true;
            }

            // อัปเดตสถานะการยืนยันอีเมล์ในฐานข้อมูล
            await _userManager.UpdateAsync(user);

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Email confirmed successfully." });
        }

        public async Task<object> CreateAddressAsync(AdressDto address)
        {
            var user = await _userManager.FindByIdAsync(address.UserId);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Fail" });
            }

            var result = _dataContext.Addresses.FirstOrDefault(a => a.UserId == user.Id);

            if (result == null)
            {
                result = new Address();
                _dataContext.Addresses.Add(result);
            }

            result.Province = address.Province;
            result.District = address.District;
            result.SubDistrict = address.SubDistrict;
            result.PostalCode = address.PostalCode;
            result.UserId = user.Id;

            await _dataContext.SaveChangesAsync();

            return Ok(result);
        }

        public async Task<object> DeleteAsync(DeleteUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                return Ok(new ResponseReport { Status = "201", Message = "Deleted Successfully" });
            }
            else
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Failed to Delete" });
            }
        }

        public async Task<object> ForgetPasswordAsync(ForgetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return Unauthorized();
            }

            if(dto.Password != dto.ConfirmPassword)
            {
                return BadRequest(new { Message = "Password and confirm password do not match" });
            }
            var changePasswordResult = await _userManager.RemovePasswordAsync(user);

            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to change password" });
            }

            changePasswordResult = await _userManager.AddPasswordAsync(user, dto.Password);

            if (!changePasswordResult.Succeeded)
            {
                return BadRequest(new { Message = "Failed to Change Password" });
            }

            return Ok(new { Message = "Password changed successfully" });
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



        public async Task<object> GetSingleUserAsync(UserNameDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);

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
                var email = await _userManager.GetEmailAsync(user);
                var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                users.Add(new { user.UserName, userRole, email, emailConfirmed });
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
                var userid = await _userManager.GetUserIdAsync(check);
                var userDto = new UserDto
                {
                    Email = check.Email,
                    Token = await _tokenService.GenerateToken(check),
                    username = loginDto.Username,
                    userid = userid
                };
                return userDto;
            }
        }

        public async Task<object> PostUserAddressAsync(UseridDto dto)
        {
            if (string.IsNullOrEmpty(dto.UserId))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "UserId is required" });
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User not found" });
            }

            var address = _dataContext.Addresses.FirstOrDefault(a => a.UserId == dto.UserId);

            if (address == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User does not have an address" });
            }

            var addressDto = new Address
            {
                Id = address.Id,
                Province = address.Province,
                District = address.District,
                SubDistrict = address.SubDistrict,
                PostalCode = address.PostalCode,
                UserId = user.Id,
            };
            return Ok(addressDto);
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

            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            string token = randomNumber.ToString();

            _memoryCache.Set("Token", token, TimeSpan.FromDays(1));

            await _userManager.UpdateAsync(createuser);

            if (!string.IsNullOrEmpty(token))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailConfirmationEmail(createuser.Email, token);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "ไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง" });
            }
            return StatusCode(StatusCodes.Status201Created, new ResponseReport { Status = "201", Message = "Create Successfully" });
        }

        public async Task<object> ResendConfirmEmailAsync(ResendEmailconfirmDto dto)
        {
            _memoryCache.Remove("Token");
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            string token = randomNumber.ToString();
            _memoryCache.Set("Token", token, TimeSpan.FromDays(1));

            if (!string.IsNullOrEmpty(token))
            {
                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailConfirmationEmail(dto.Email, token);
            }
            else
            {
                // กรณีไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "ไม่ได้รับค่า emailConfirmationUrl ที่ถูกต้อง" });
            }
            return StatusCode(StatusCodes.Status202Accepted);
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

                var htmlContent = "<div style=\"text-align: center;\">";
                htmlContent += "<p><strong><h1 style=\"font-size:2em; \">Confirm Email</h1></strong></p>";
                htmlContent += "<p>Thank you for registering! Please confirm your email address by using the following token:</p>";
                htmlContent += $"<p><strong><h1 style=\"font-size:4em; \">{cachedToken}</h1></strong></p>";
                htmlContent += "</div>";


                var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
                await _sendGridClient.SendEmailAsync(emailMessage);

                ConfirmUserDto confirmUserDto = new()
                {
                    Email = email,
                    EmailConfirmationToken = cachedToken
                };

            }
        }

    }
}
