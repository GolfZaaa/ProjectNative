using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectNative.Data;
using ProjectNative.DTOs.CartDto;
using ProjectNative.DTOs.OrderDto;
using ProjectNative.Extenstions;
using ProjectNative.Models;
using ProjectNative.Models.CartAccount;
using ProjectNative.Models.OrderAccount;
using Response = ProjectNative.Models.ResponseReport;
using ProjectNative.Services.IService;
using SendGrid;
using SendGrid.Helpers.Mail;
using Stripe;

namespace ProjectNative.Services
{
    public class CartUsersService : ControllerBase, ICartUsersService
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUploadFileService _uploadFileService;
        private readonly SendGridClient _sendGridClient;
        private readonly IConfiguration _configuration;

        public CartUsersService(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, IUploadFileService uploadFileService, SendGridClient sendGridClient, IConfiguration configuration)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _uploadFileService = uploadFileService;
            _sendGridClient = sendGridClient;
            _configuration = configuration;
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
                shopCart = cart;
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
            if (getCartUser == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "Invalid request body" });
            }

            var user = await _userManager.FindByIdAsync(getCartUser.userid);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Username Not Found" });
            }

            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(a => a.ProductImages)
                .SingleOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new ResponseReport { Status = "404", Message = "Cart Not Found" });
            }

            cart.TotalPrice = 0;
            foreach (var item in cart.Items)
            {
                cart.TotalPrice += item.Product.Price * item.Amount;
            }

            var cartResponse = cart.ToCartResponse();

            return cartResponse;
        }

        public async Task<object> DeleteItemToCartAsync(DeleteProductInCart delete)
        {
            var cart = await _dataContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == delete.UserId);

            if (cart == null)
            {
                return BadRequest();
            }

            var item = cart.Items.FirstOrDefault(item => item.ProductId == delete.ProductId);
            if (item == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "item is null" });
            }

            item.Amount -= delete.Amount; //ลบสินค้าออกตามจำนวนที่ส่งมา
            if (item.Amount <= 0)
            {
                cart.Items.Remove(item); //ถ้าจำนวนสินค้ามันเป็น 0 หรือน้อยกว่า ให้ลบสินค้านั้นทิ้งไป
            }

            await _dataContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success" });
        }

        public async Task<object> DeleteItemToCartallAsync(DeleteProductInCartDtoAllcs delete)
        {
            var cart = await _dataContext.Carts
                            .Include(c => c.Items)
                            .FirstOrDefaultAsync(c => c.UserId == delete.UserId);

            if (cart == null)
            {
                return BadRequest();
            }

            var itemsToRemove = cart.Items.Where(item => item.ProductId == delete.ProductId).ToList();

            if (itemsToRemove.Count == 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ResponseReport { Status = "400", Message = "item is null" });
            }

            foreach (var item in itemsToRemove)
            {
                cart.Items.Remove(item); // ลบสินค้าออกจากตะกร้า
            }

            await _dataContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new ResponseReport { Status = "200", Message = "Success" });
        }


        private async Task<(string errorMessge, string imageNames)> UploadImageMainAsync(IFormFile formfile)
        {
            var errorMessge = string.Empty;
            var imageName = string.Empty;

            if (_uploadFileService.IsUpload(formfile))
            {
                errorMessge = _uploadFileService.Validation(formfile);
                if (errorMessge is null)
                {
                    imageName = await _uploadFileService.UploadImages(formfile);
                }
            }

            return (errorMessge, imageName);
        }


        private async Task<PaymentIntent> CreatePaymentIntent(Order order)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var service = new PaymentIntentService();
            var intent = new PaymentIntent();

            //สร้างรายการใหม่
            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)order.TotalAmount * 100, // ยอดเงินเท่าไร
                    Currency = "THB", // สกุลเงิน 
                    PaymentMethodTypes = new List<string> { "card" } // วิธีการจ่าย
                };
                intent = await service.CreateAsync(options); // รหัสใบส่งของ
            };

            return intent; // ส่งใบส่งของออกไป
        }

        public async Task<object> CreateOrderAsyncReal([FromForm] OrderDto orderDTO)
        {
            var check = await _dataContext.Addresses.FirstOrDefaultAsync(a => a.UserId == orderDTO.UserId);

            if (check == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "Address not Found" });
            }
            var cart = await RetrieveCart(orderDTO.UserId);
            if (cart == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "Cart not Found" });
            }
            var user = await _dataContext.Users.SingleOrDefaultAsync(a => a.Id == orderDTO.UserId);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "400", Message = "User not Found" });
            }
            var order = new Order
            {
                AddressId = check.Id,
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.PendingApproval,
            };
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Amount,
                    Price = item.Product.Price,
                    Order = order
                };
                order.OrderItems.Add(orderItem);
                var product = await _dataContext.Products.FirstOrDefaultAsync(a => a.Id == item.ProductId);
                if (product != null)
                {
                    product.QuantityInStock -= item.Amount;
                }
            }
            order.TotalAmount = order.GetTotalAmount();
            _dataContext.Orders.Add(order);
            await _dataContext.SaveChangesAsync();
            _dataContext.Carts.RemoveRange(cart);
            if (orderDTO.PaymentMethod == Models.OrderAccount.PaymentMethod.CreditCard)
            {
                var intent = await CreatePaymentIntent(order);
                if (!string.IsNullOrEmpty(intent.Id))
                {
                    order.PaymentIntentId = intent.Id; // เอาใบส่งของใส่ในใบสั่งซื้อ
                    order.ClientSecret = intent.ClientSecret; // เอารหัสลับใส่ในใบสั่งซื้อ
                };
            }
            else
            {
                (string errorMessgeMain, string imageNames) =
                await UploadImageMainAsync(orderDTO.OrderImage);
                order.OrderImage = imageNames;
            }

            await _dataContext.SaveChangesAsync();

            var orderItemsHtml = "";
            foreach (var item in order.OrderItems)
            {
                orderItemsHtml += $@"
                    <div style=""border: 1px solid #ccc; padding: 10px; margin: 10px;"">
                        <p><strong>Product:</strong> {item.Product.Name}</p>
                        <p><strong>Quantity:</strong> {item.Quantity}</p>
                        <p><strong>Price:</strong> {item.Price}</p>
                    </div>";
            }

            var from = new EmailAddress("64123250113@kru.ac.th", "Golf");
            var to = new EmailAddress(user.Email);
            var subject = "Order Confirmation";
            var htmlContent = $@"
    <div style=""width: 100%; max-width: 600px; margin: 0 auto; font-family: Arial, sans-serif; border: 1px solid #ccc; padding: 20px;"">
        <div style=""text-align: center;"">
            <img src=""https://api.freelogodesign.org/assets/thumb/logo/a17b07eb64d341ffb1e09392aa3a1698_400.png"" alt=""Company Logo"" style=""max-width: 150px; margin-bottom: 20px;"">
            <h2 style=""font-size: 24px; color: #333; margin-bottom: 10px;"">Order Receipt</h2>
            <p style=""font-size: 16px; color: #666;"">Thank you for shopping with us!</p>
        </div>

        <hr style=""border: 1px solid #ccc; margin: 20px 0;"">

        <div style=""margin-bottom: 20px;"">
            <h3 style=""font-size: 20px; color: #333; margin-bottom: 10px;"">Order Details</h3>
            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Order Date:</strong> {order.OrderDate}</p>
        </div>

        <div style=""margin-bottom: 20px;"">
            <h3 style=""font-size: 20px; color: #333; margin-bottom: 10px;"">Shipping Address</h3>
            <p>{check.SubDistrict}, {check.District}, {check.Province}, {check.PostalCode}</p>
        </div>

        <div style=""margin-bottom: 20px;"">
            <h3 style=""font-size: 20px; color: #333; margin-bottom: 10px;"">Order Items</h3>
            {orderItemsHtml}
        </div>

        <hr style=""border: 1px solid #ccc; margin: 20px 0;"">

        <div style=""text-align: right;"">
            <p style=""font-size: 18px; color: #333;""><strong>Total Amount:</strong> {order.TotalAmount}</p>
        </div>

        <div style=""text-align: center; margin-top: 20px;"">
            <p style=""font-size: 16px; color: #666;"">Thank you for your purchase!</p>
          </div>
            </div>";


            var emailMessage = MailHelper.CreateSingleEmail(from, to, subject, htmlContent, htmlContent);
            await _sendGridClient.SendEmailAsync(emailMessage);

            return order;
        }




    }
}
