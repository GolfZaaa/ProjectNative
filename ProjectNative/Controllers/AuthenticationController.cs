using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.DTOs;
using ProjectNative.Models;
using ProjectNative.Services;
using ProjectNative.Services.IService;
using System.Data;
using System.Security.Claims;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAccountService _accountService;

        public AuthenticationController(TokenService tokenService, UserManager<ApplicationUser> userManager, IAccountService accountService)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _accountService = accountService;

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

            //var result = await _userManager.FindByNameAsync(username);
            //var userRole = await _userManager.GetRolesAsync(result);
            //var email = await _userManager.GetEmailAsync(result);
            //var securitystamp = await _userManager.GetSecurityStampAsync(result);
            //var userid = await _userManager.GetUserIdAsync(result);
            //var name = await _userManager.GetUserNameAsync(result);
            //var userData = new
            //{
            //    name,
            //    userRole,
            //    securitystamp,
            //    email,
            //    userid
            //};
            //return Ok(userData);

            var result = await _accountService.GetSingleUserAsync(username);

            return Ok(result);

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
        {
            var result = await _accountService.RegisterAsync(registerDto);

            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromForm] LoginDto loginDto)
        {
            var result = await _accountService.LoginAsync(loginDto);

            return Ok(result);
        }

        [HttpDelete("[action]")]
        public async Task<IActionResult> Delete([FromForm] string username)
        {
            var result = await _accountService.DeleteAsync(username);
            return Ok(result);
        }

        //[HttpGet("TestAdminRole"), Authorize(Roles = "Admin")]
        //public IActionResult test()
        //{
        //    return Ok("Authorize Success");
        //}

        [HttpGet("[action]"), Authorize]
        public async Task<IActionResult> GetMyName()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var roles = await _userManager.GetRolesAsync(user);


            var userData = new
            {
                user.UserName,
                roles,
                userEmail,

            };
            return Ok(userData);
        }

        [HttpGet("[action]"), Authorize]
        public async Task<IActionResult> GetToken()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            if (accessToken == null)
            {
                accessToken = "Not Login";
            }
            return Ok(accessToken);
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
                return BadRequest(changePasswordResult.Errors);
            }

            // เปลี่ยนรหัสผ่านสำเร็จ
            return Ok(StatusCode(StatusCodes.Status200OK, new Response { Status = "200", Message = "ChangePassword Successfuly" }));
        }


        [HttpPost("ChangeEmail")]
        public async Task<bool> ChangeUserEmail(string userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // ไม่พบผู้ใช้งานที่มี userId ที่ระบุ
                return false;
            }

            var result = await _userManager.SetEmailAsync(user, newEmail);
            if (!result.Succeeded)
            {
                // เกิดข้อผิดพลาดในการตั้งค่าอีเมลใหม่
                return false;
            }

            // สร้างโทเคนยืนยันอีเมลใหม่
            var emailConfirmationToken = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var changeEmailResult = await _userManager.ChangeEmailAsync(user, newEmail, emailConfirmationToken);
            if (!changeEmailResult.Succeeded)
            {
                // เกิดข้อผิดพลาดในการเปลี่ยนอีเมล
                return false;
            }

            return true;
        }

    }
}
