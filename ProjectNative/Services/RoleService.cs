using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.DTOs;
using ProjectNative.Models;
using ProjectNative.Services.IService;

namespace ProjectNative.Services
{
    public class RoleService : ControllerBase, IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<object> CreateAsync(RoleDto roleDto)
        {
            var identityRole = new IdentityRole
            {
                Name = roleDto.Name,
                NormalizedName = _roleManager.NormalizeKey(roleDto.Name)
            };


            var result = await _roleManager.CreateAsync(identityRole);


            if (!result.Succeeded) return ResultValidation(result);


            return StatusCode(201);
        }

        private object ResultValidation(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem();
        }

        public async Task<object> DeleteAsync(RoleDto roleDto)
        {
            var identityRole = await _roleManager.FindByNameAsync(roleDto.Name);


            if (identityRole == null) return NotFound();

            //ตรวจสอบมีผู้ใช้บทบาทนี้หรือไม่
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleDto.Name);
            if (usersInRole.Count != 0) return BadRequest();

            var result = await _roleManager.DeleteAsync(identityRole);


            if (!result.Succeeded) return ResultValidation(result);


            return StatusCode(201);
        }

        public async Task<List<IdentityRole>> GetAsync()
        {
            var result = await _roleManager.Roles.ToListAsync();
            return result;
        }

        public async Task<object> UpdateAsync(RoleUpdateDto roleUpdateDto)
        {
            var identityRole = await _roleManager.FindByNameAsync(roleUpdateDto.Name);


            if (identityRole == null) return NotFound();


            identityRole.Name = roleUpdateDto.UpdateName;
            identityRole.NormalizedName = _roleManager.NormalizeKey(roleUpdateDto.UpdateName);


            var result = await _roleManager.UpdateAsync(identityRole);


            if (!result.Succeeded) return ResultValidation(result);


            return StatusCode(201);

        }
    }
}
