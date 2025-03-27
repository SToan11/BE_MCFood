using ASM_C_5.Data;
using ASM_C_5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASM_C_5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ASM_C_5Context _context;

        public OrdersController(ASM_C_5Context context)
        {
            _context = context;
        }

        // Lấy danh sách đơn hàng của người dùng
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Food)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Combo)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return Ok(orders);
        }

        // Tạo đơn hàng (Checkout)
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Giỏ hàng trống.");

            decimal totalPrice = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var cartItem in cart.CartItems)
            {
                decimal price = cartItem.Price; // Lấy trực tiếp từ CartItem
                totalPrice += price * cartItem.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    FoodID = cartItem.FoodID,
                    ComboID = cartItem.ComboID,
                    Quantity = cartItem.Quantity,
                    UnitPrice = price,
                    ProductName = cartItem.ProductName
                });
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                PaymentMethod = request.PaymentMethod,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                OrderDetails = orderDetails
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã đặt hàng thành công.", order });
        }
    }

    // DTO để nhận dữ liệu checkout
    public class CheckoutRequest
    {
        public string PaymentMethod { get; set; }
    }
}
