using ProjectNative.DTOs;
using ProjectNative.DTOs.AccConfirm;
using ProjectNative.DTOs.Address;

namespace ProjectNative.Services.IService
{
    public interface IAccountService
    {
        Task<List<Object>> GetUsersAsync();
        Task<Object> LoginAsync(LoginDto loginDto);
        Task<Object> RegisterAsync(RegisterDto registerDto);
        Object GetMe();
        Task<Object> GetSingleUserAsync(UserNameDto dto);
        Task<Object> DeleteAsync(DeleteUserDto dto);
        Task<Object> ConfirmEmailAsync(ConfirmUserDto confirmUserDto);
        Task<Object> ResendConfirmEmailAsync(ResendEmailconfirmDto dto);
        Task<Object> ChangePasswordAsync(ChangePasswordDto dto);
        Task<Object> ForgetPasswordAsync(ForgetPasswordDto dto);
        Task<Object> ChangeUserEmailAsync(ChangeUserEmailDto dto);
        Task<Object> ChangeUserNameAsync(ChangeUserNameDto dto);
        Task<Object> CreateAddressAsync(AdressDto address);
        Task<Object> PostUserAddressAsync(UseridDto dto);

    }
}
