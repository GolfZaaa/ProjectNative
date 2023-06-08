using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.CartDto;
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
            var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.Id == addCartDTO.userid);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "User not found");
            }

            var product = await _dataContext.Products.SingleOrDefaultAsync(e => e.Id == addCartDTO.productId);

            //var product = await _dataContext.Products.Include(e => e.ProductImages).SingleOrDefaultAsync(e => e.Id == addCartDTO.productId);


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
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "200", Message = "Add Product to Cart Successfuly" });

            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "404", Message = "Fail Add Product to Cart" });
            }
        }
    }
}
