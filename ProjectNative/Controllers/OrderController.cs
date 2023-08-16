using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Models;
using ProjectNative.Models.OrderAccount;
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

        public OrderController(DataContext dataContext, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetOrder()
        {
            var order = await _dataContext.Orders.Include(o => o.OrderItems)
                .Select(o => new
                {
                    o.Id,
                    o.AddressId,
                    o.Address,
                    o.ClientSecret,
                    o.OrderDate,
                    OrderItem = o.OrderItems.Select(o => new
                    {
                        o.ProductId,
                        o.Product,
                        o.Quantity,
                        o.Price,
                    }),
                    o.TotalAmount,
                })
                .ToListAsync();

            return Ok(order);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GetOrdersByUsername(CreateOrderDTO orderDTO)
        {
            //var address = await _dataContext.Addresses.FirstOrDefaultAsync(a => a.Id == orderDTO.AddressId);

            var user = await _userManager.Users.FirstOrDefaultAsync(a => a.UserName == orderDTO.UserName);

            var orders = await _dataContext.Orders
                .Include(o => o.OrderItems)
                .Include(a => a.Address)
                .Where(o => o.Address.UserId == user.Id)
                .ToListAsync();
            return Ok(orders);
        }

 


    }
}
