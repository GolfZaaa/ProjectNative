using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Models;
using ProjectNative.Models.OrderAccount;
using ProjectNative.Services.IService;
using SendGrid.Helpers.Mail;

namespace ProjectNative.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public OrderController(DataContext dataContext, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IOrderService orderService)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _orderService = orderService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOrder()
        {
            var result = await _orderService.GetOrderAsync();
            return Ok(result);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetOrdersByUsername(CreateOrderDTO orderDTO)
        {
            var result = await _orderService.GetOrdersByUsernameAsync(orderDTO);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> GetOrderByUserId(GetOrderByUserIdDTO dto)
        {
            var result = await _orderService.GetOrderByUserIdAsync(dto);
            return Ok(result);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatus dto, OrderStatus newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(dto, newStatus);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            return Ok(result);
        }

    }
}