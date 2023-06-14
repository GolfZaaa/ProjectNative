
using Hanssens.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ProjectNative.DTOs;
using ProjectNative.Models;
using ProjectNative.Services;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Helpers.Mail; 

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        LocalStorage storage = new ();
        private readonly TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly SendGridClient _sendGridClient;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        public AuthenticationController(TokenService tokenService, UserManager<ApplicationUser> userManager, IAccountService accountService,
            IConfiguration configuration, SendGridClient sendGridClient, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _accountService = accountService;
            _configuration = configuration;
            _sendGridClient = sendGridClient;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _accountService.GetUsersAsync();
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetSingleUser(string username)
        {
            var result = await _accountService.GetSingleUserAsync(username);
            return Ok(result);

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
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

            //var token = await _userManager.GenerateEmailConfirmationTokenAsync(createuser);

            // เก็บ token ในฐานข้อมูลหรือตารางเฉพาะสำหรับการยืนยันอีเมล์ของผู้ใช้งาน
            //createuser.EmailConfirmationToken = token;

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

                await ConfirmEmail(email, cachedToken);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        {
            var result = await _accountService.LoginAsync(loginDto);

            return Ok(result);
        }

        //[HttpPost("[action]")]
        //public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        //{
        //    var check = await _userManager.FindByNameAsync(loginDto.Username);

        //    if (check == null || !await _userManager.CheckPasswordAsync(check, loginDto.Password))
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Invalid username or password. Please try again." });
        //    }
        //    else
        //    {
        //        var otp = Guid.NewGuid().ToString();
        //        _memoryCache.Set("Otp", otp, TimeSpan.FromMinutes(3));
        //        await SendEmailConfirmationEmail(check.Email, otp);
        //    }


        //    //if (!string.IsNullOrEmpty(check.Email))
        //    //{
        //    //    // สร้าง token สำหรับการยืนยันอีเมล์
        //    //    var otp = Guid.NewGuid().ToString();
        //    //    _memoryCache.Set("Otp", otp, TimeSpan.FromMinutes(3));
        //    //    await SendEmailConfirmationEmail(createuser.Email, token);
        //    //}

        //    return Ok(check);
        //}



        [HttpPost("[action]")]
        public async Task<IActionResult> ConfirmEmail([FromForm] string EmailConfirmationToken, string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid user." });
            }

            var token = _memoryCache.Get<string>("Token"); // รับค่า token จาก memory cache

            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "token null." });
            }

            if (string.IsNullOrEmpty(EmailConfirmationToken))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "EmailConfirmationToken null ." });
            }

            // เช็คว่าโทเค็นที่ผู้ใช้กรอกตรงกับโทเค็นที่เก็บในแคชไหม
            if (EmailConfirmationToken != token)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid email confirmation token." });
            }

            // อัปเดตสถานะการยืนยันอีเมล์ในฐานข้อมูล
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Email confirmed successfully." });
        }




        [HttpGet("[action]")]
        public async Task<IActionResult> check()
        {
            var data = _memoryCache.Get("Token");
            return Ok(data);
        }

        




        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete([FromForm] string username)
        {
            var result = await _accountService.DeleteAsync(username);
            return Ok(result);
        }


        [HttpGet("[action]"), Authorize]
        public async Task<IActionResult> GetMyName()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var iduser = await _userManager.GetUserIdAsync(user);

            var userEmail = await _userManager.GetEmailAsync(user);

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            if (accessToken == null)
            {
                accessToken = "Not Login";
            }

            var userData = new
            {
                user.UserName,
                roles,
                userEmail,
                iduser,
                accessToken
            };

            return Ok(userData);
        }



        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync(string password, string newPassword)
        {
            //ใช้ไม่ได้
            //var user = await _userManager.GetUserAsync(User);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                // ไม่พบผู้ใช้ที่เข้าสู่ระบบ
                return Unauthorized();
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, password, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                // ไม่สามารถเปลี่ยนรหัสผ่านได้
                return StatusCode(StatusCodes.Status400BadRequest,new ResponseReport { Status = "400" , Message = "The password you entered is incorrect. Please try again." });
            }
            //เช็ค Error
            if (password == newPassword)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "400", Message = "The new password is the same as the current password you are using. Please enter a password that is different from the current one." });
            }
            // เปลี่ยนรหัสผ่านสำเร็จ
            return Ok(StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "ChangePassword Successfuly" }));
        }


        [HttpPost("ChangeEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeUserEmail(string NewEmail)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                // หา User ไม่เจอ
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            if(user.Email == NewEmail)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "400", Message = "The new Email is the same as the current Email you are using. Please enter a Email that is different from the current one." });
            }
            var result = await _userManager.SetEmailAsync(user, NewEmail);
            if (!result.Succeeded)
            {
                // เกิดข้อผิดพลาดในการตั้งค่าอีเมลใหม่
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Fail to SetEmail" });
            }

            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Change Email Successfuly" });
        }


        [HttpPost("[action]"),Authorize]
        public async Task<IActionResult> ChangeUserName (string NewUserName)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            if(string.IsNullOrEmpty(User.Identity.Name))
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "User Not Found" });
            }

            if (user.UserName == NewUserName)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "400", Message = "The new UserName is the same as the current UserName you are using. Please enter a UserName that is different from the current one." });
            }

            var result = await _userManager.SetUserNameAsync(user, NewUserName);
            if(!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Fail to SetUserName" });
            }
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Change UserName Successfully, please logout and login to get token again" });
        }
 

    }
}
