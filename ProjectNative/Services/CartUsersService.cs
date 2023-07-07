using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.CartDto;
using ProjectNative.DTOs.ProductDto.Response;
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.Services.IService;

namespace ProjectNative.Services
{
    public class CartUsersService : ControllerBase, ICartUsersService
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartUsersService(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        private string GenerateID() => Guid.NewGuid().ToString("N");


        public async Task<object> AddItemToCartAsync(AddCartRequestDTO addCartDTO)
        {
            var carttest = await RetrieveCart(addCartDTO.userid);

            var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.Id == addCartDTO.userid);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "User not found");
            }
            var product = await _dataContext.Products.SingleOrDefaultAsync(e => e.Id == addCartDTO.productId);
            if (product == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "Product not found");
            }
            var shopCart = _dataContext.Carts.FirstOrDefault(x => x.UserId == addCartDTO.userid);
            if (shopCart == null)
            {
                Cart cart = new Cart { Id = GenerateID(), UserId = addCartDTO.userid };
                await _dataContext.Carts.AddAsync(cart);
                await _dataContext.SaveChangesAsync();
                shopCart = cart; // Set the value of shopCart to the newly created cart
            }
            shopCart.AddItem(product, addCartDTO.amount);
            try
            {
                await _dataContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Add Product to Cart Successfuly" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Fail Add Product to Cart" });
            }
        }

        private async Task<Cart> RetrieveCart(string accountId)
        {
            var cart = await _dataContext.Carts
                   .Include(i => i.Items)
                   .ThenInclude(p => p.Product)
                   .SingleOrDefaultAsync(x => x.UserId == accountId);
            return cart;
        }



        public async Task<object> GetCartByUsernameAsync(GetCartUserDto getCartUser)
        {
            var user = await _userManager.FindByEmailAsync(getCartUser.email);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Username Not Found" });
            }
            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Cart Not Found" });
            }
            // แปลงข้อมูลใน cart ให้เป็น ProductResponse
            var productResponses = cart.Items.Select(item => ProductResponse.FromProduct(item.Product)).ToList();
            // สร้างตัวแปรเก็บข้อมูลการตอบกลับ
            var response = new
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Products = productResponses
            };
            return response;
        }



    }
}
