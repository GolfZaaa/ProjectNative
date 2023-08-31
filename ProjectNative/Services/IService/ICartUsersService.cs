using Microsoft.AspNetCore.Mvc;
using ProjectNative.DTOs.CartDto;
using ProjectNative.DTOs.OrderDto;

namespace ProjectNative.Services.IService
{
    public interface ICartUsersService
    {
        Task<Object> AddItemToCartAsync(AddCartRequestDTO addCartDTO);
        Task<Object> GetCartByUsernameAsync([FromBody] GetCartUserDto getCartUser);
        Task<Object> DeleteItemToCartAsync(DeleteProductInCart delete);
        Task<Object> DeleteItemToCartallAsync(DeleteProductInCartDtoAllcs delete);
        Task<Object> CreateOrderAsyncReal([FromForm] OrderDto orderDTO);
    }
}
