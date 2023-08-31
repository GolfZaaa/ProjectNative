using ProjectNative.DTOs;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Models.OrderAccount;

namespace ProjectNative.Services.IService
{
    public interface IOrderService
    {
        Task<Object> GetOrderByUserIdAsync(GetOrderByUserIdDTO dto);
        Task<Object> GetOrderAsync();
        Task<Object> GetOrdersByUsernameAsync(CreateOrderDTO orderDTO);
        Task<Object> UpdateOrderStatusAsync(UpdateOrderStatus dto, OrderStatus newStatus);
        Task<Object> DeleteOrderAsync(int id);
    }
}
