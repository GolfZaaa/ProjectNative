using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Models;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Services.IService;
using ProjectNative.SettingUrl;

namespace ProjectNative.Services
{
    public class OrderService : ControllerBase, IOrderService
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public OrderService(DataContext dataContext, UserManager<ApplicationUser> userManager)
        {
            _dataContext = dataContext;
            _userManager = userManager;
        }

        public async Task<object> DeleteOrderAsync(int id)
        {
            var result = await _dataContext.Orders.FindAsync(id);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Not found Order." });
            }

            _dataContext.Orders.Remove(result);
            await _dataContext.SaveChangesAsync();

            return Ok(new ResponseReport { Status = "200", Message = "Order deleted successfully." });
        }

        public async Task<object> GetOrderAsync()
        {
            var order = await _dataContext.Orders.Include(o => o.OrderItems)
                           .Select(o => new
                           {
                               o.Id,
                               o.AddressId,
                               o.Address,
                               o.ClientSecret,
                               o.OrderDate,
                               o.OrderImage,
                               OrderItem = o.OrderItems.Select(o => new
                               {
                                   o.ProductId,
                                   o.Product,
                                   o.Quantity,
                                   o.Price,
                                   o.ReviewStatus
                               }),
                               o.TotalAmount,
                               o.OrderStatus,
                           })
                           .ToListAsync();

            return order;
        }

        public async Task<object> GetOrderByUserIdAsync(GetOrderByUserIdDTO dto)
        {
            var order = await _dataContext.Orders.Include(o => o.OrderItems)
                .Where(o => o.Address.UserId == dto.UserId)
                .Select(o => new
                {
                    o.Id,
                    o.AddressId,
                    o.Address,
                    o.ClientSecret,
                    o.OrderDate,
                    OrderItem = o.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        oi.ProductId,
                        oi.Product,
                        oi.Quantity,
                        oi.Price,
                        oi.ReviewStatus,
                    }),
                    TotalAmount = o.GetTotalAmount(),
                    o.PaymentIntentId,
                    o.OrderStatus,
                })
                .ToListAsync();
            return order;
        }

        public async Task<object> GetOrdersByUsernameAsync(CreateOrderDTO orderDTO)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.UserName == orderDTO.UserName);

            var orders = await _dataContext.Orders
                .Include(o => o.OrderItems)
                .Include(a => a.Address)
                .Where(o => o.Address.UserId == user.Id)
                .ToListAsync();
            return orders;
        }

        public async Task<object> UpdateOrderStatusAsync(UpdateOrderStatus dto, OrderStatus newStatus)
        {
            var order = await _dataContext.Orders.FindAsync(dto.Id);

            if (order == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Not found Order." });
            }

            if (order.OrderStatus != OrderStatus.SuccessfulPayment)
            {
                order.OrderStatus = OrderStatus.SuccessfulPayment;
                await _dataContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success to Update Status." });
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Fall to Update Status." });

            }
        }
    }
}
