using ProjectNative.DTOs;
using ProjectNative.DTOs.AccConfirm;

namespace ProjectNative.Services.IService
{
    public interface IAccountService
    {
        Task<List<Object>> GetUsersAsync();
        Task<Object> LoginAsync(LoginDto loginDto);
        Task<Object> RegisterAsync(RegisterDto registerDto);
        Object GetMe();
        Task<Object> GetSingleUserAsync(string username);
        Task<Object> DeleteAsync(string username);
        Task<Object> ConfirmEmailAsync(ConfirmUserDto confirmUserDto);
    }
}
