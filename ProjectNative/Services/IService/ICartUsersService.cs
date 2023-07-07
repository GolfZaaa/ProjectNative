using ProjectNative.DTOs.CartDto;

namespace ProjectNative.Services.IService
{
    public interface ICartUsersService
    {
        Task<Object> AddItemToCartAsync(AddCartRequestDTO addCartDTO);
        Task<Object> GetCartByUsernameAsync(GetCartUserDto getCartUser);
    }
}
