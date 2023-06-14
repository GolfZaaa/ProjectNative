using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProjectNative.DTOs;
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

        public AccountService(UserManager<ApplicationUser> userManager, TokenService tokenService, IHttpContextAccessor httpContextAccessor, RoleManager<IdentityRole> roleManager,
            SendGridClient sendGridClient)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _roleManager = roleManager;
            _sendGridClient = sendGridClient;
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
            var userDto = new UserDto
            {
                Email = check.Email,
                Token = await _tokenService.GenerateToken(check),
            };

            return (userDto);
        }

        public async Task<object> RegisterAsync(RegisterDto registerDto)
        {
            // เช็ค error
            var check = await _userManager.FindByEmailAsync(registerDto.Email);

            if (check != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "404", Message = "This e-mail has already been used." });
            }

            #region ตรวจสอบว่า Role ที่ได้รับมาไม่มีในฐานข้อมูล
            var roleExists = await _roleManager.RoleExistsAsync(registerDto.Role);
            if (!roleExists)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "404", Message = "The specified role does not exist." });
            }
            #endregion

            // สร้าง user
            var createuser = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                
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


            #region SendGrid
            //โดยโทเค็นนี้จะถูกใช้ในการยืนยันอีเมล์ผ่าน URL ที่จะส่งให้กับผู้ใช้งานเพื่อให้ผู้ใช้ที่สมัครคลิกเพื่อยืนยันอีเมล์ของตนเอง
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(createuser);


            // สร้าง URL ไว้สำหรับยืนยันอีเมล์
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null && _httpContextAccessor.HttpContext.Request != null)
            {
                // กำหนดค่า URL ตามที่คุณต้องการ
                var emailConfirmationUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" +
                    _httpContextAccessor.HttpContext.Request.Host +
                    _httpContextAccessor.HttpContext.Request.PathBase +
                    Url.Link("ConfirmEmail", new { userId = createuser.Id, token = emailConfirmationToken });

                // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
                await SendEmailConfirmationEmail(createuser.Email, emailConfirmationUrl);
            }
            else
            {
                // กรณีไม่พบ HttpContext หรือ Request ให้จัดการตามที่เหมาะสม
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Error" });
            }

            // ส่งอีเมล์ยืนยันอีเมล์ไปยังผู้ใช้
            #endregion

            return StatusCode(StatusCodes.Status201Created,new ResponseReport { Status = "201", Message = " Create Successfuly"});
        }

        private async Task SendEmailConfirmationEmail(string email,string emailConfirmationUrl)
        {
            var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
            var to = new EmailAddress(email);
            var subject = "Thank you";
            var htmlContent = "Thank you for Register";
            var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, "WelCome to my Application");
            await _sendGridClient.SendEmailAsync(emailMessage);
        }

    }
}
