using ASM_C_5.Data;
using ASM_C_5.DTOS;
using ASM_C_5.Models;
using ASM_C_5.Models.vnpay;
using ASM_C_5.Services.Vnpay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASM_C_5.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController] 
    public class CheckoutController : ControllerBase
    {
        private readonly ASM_C_5Context _context;
        private readonly IVnPayService _vnPayService;


        public CheckoutController(ASM_C_5Context context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            using var transaction = await _context.Database.BeginTransactionAsync();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "Không tìm thấy thông tin khách hàng!"));
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(new ApiResponse(400, "Giỏ hàng trống hoặc không tồn tại!"));
            }

            // Lấy danh sách ID của Food và Combo để kiểm tra tồn tại và giá
            var foodIds = cart.CartItems.Where(ci => ci.FoodID.HasValue).Select(ci => ci.FoodID.Value).ToList();
            var comboIds = cart.CartItems.Where(ci => ci.ComboID.HasValue).Select(ci => ci.ComboID.Value).ToList();

            var foodItems = await _context.FoodItems.Where(f => foodIds.Contains(f.FoodID)).ToDictionaryAsync(f => f.FoodID);
            var comboItems = await _context.ComboItems.Where(c => comboIds.Contains(c.ComboID)).ToDictionaryAsync(c => c.ComboID);

            // Kiểm tra hợp lệ của sản phẩm trong giỏ hàng
            foreach (var item in cart.CartItems)
            {
                if (item.ComboID.HasValue)
                {
                    if (!comboItems.TryGetValue(item.ComboID.Value, out var combo) || combo.Price != item.Price)
                    {
                        return BadRequest(new ApiResponse(400, $"Sản phẩm {item.ProductName} không còn tồn tại hoặc giá đã thay đổi!"));
                    }
                }
                else if (item.FoodID.HasValue)
                {
                    if (!foodItems.TryGetValue(item.FoodID.Value, out var food) || food.Price != item.Price)
                    {
                        return BadRequest(new ApiResponse(400, $"Sản phẩm {item.ProductName} không còn tồn tại hoặc giá đã thay đổi!"));
                    }
                }
            }

            var totalAmount = cart.CartItems.Sum(i => i.Quantity * i.Price);
            var totalItems = cart.CartItems.Sum(i => i.Quantity);

            var order = new Order
            {
                UserId = userId,
                CartId = cart.CartId,
                TotalPrice = totalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatus.Pending.ToString()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderDetails = cart.CartItems.Select(item => new OrderDetail
            {
                OrderID = order.OrderId,
                FoodID = item.FoodID,
                ComboID = item.ComboID,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                ProductName = item.ProductName
            }).ToList();

            _context.OrderDetails.AddRange(orderDetails);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new ApiResponse<OrderResponse>(200, "Thanh toán thành công!", new OrderResponse
            {
                OrderId = order.OrderId,
                Customer = new CustomerInfo
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                Products = orderDetails.Select(o => new OrderProductInfo
                {
                    ProductName = o.ProductName,
                    ComboId = o.ComboID,
                    Quantity = o.Quantity,
                    UnitPrice = o.UnitPrice,
                    TotalPrice = o.Quantity * o.UnitPrice
                }).ToList(),
                TotalItems = totalItems,
                TotalAmount = totalAmount,
                PaymentMethod = request.PaymentMethod,
                Status = order.Status,
                CreatedAt = order.CreatedDate
            }));
        }



        public class ApiResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; }

            public ApiResponse(int statusCode, string message)
            {
                StatusCode = statusCode;
                Message = message;
            }
        }

        public class ApiResponse<T> : ApiResponse
        {
            public T Data { get; set; }

            public ApiResponse(int statusCode, string message, T data) : base(statusCode, message)
            {
                Data = data;
            }
        }

        public enum OrderStatus
        {
            Pending,
            Processing,
            Completed,
            Cancelled
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                // Get order with details in a single query
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    return NotFound(new ApiResponse(404, "Không tìm thấy đơn hàng!"));
                }

                // Get user info
                var user = await _context.Users.FindAsync(order.UserId);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, "Không tìm thấy thông tin khách hàng!"));
                }

                // Calculate totals
                var totalItems = order.OrderDetails.Sum(o => o.Quantity);

                return Ok(new ApiResponse<OrderDetailsResponse>(200, "Lấy thông tin đơn hàng thành công!", new OrderDetailsResponse
                {
                    OrderId = order.OrderId,
                    Status = order.Status,
                    PaymentMethod = order.PaymentMethod,
                    CreatedAt = order.CreatedDate,
                    Customer = new CustomerInfo
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    Products = order.OrderDetails.Select(o => new OrderProductInfo
                    {
                        ProductName = o.ProductName,
                        ComboId = o.ComboID,
                        FoodId = o.FoodID,
                        Quantity = o.Quantity,
                        UnitPrice = o.UnitPrice,
                        TotalPrice = o.Quantity * o.UnitPrice
                    }).ToList(),
                    TotalItems = totalItems,
                    TotalAmount = order.TotalPrice
                }));
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving order details for OrderId: {OrderId}", orderId);
                return StatusCode(500, new ApiResponse(500, "Đã xảy ra lỗi khi lấy thông tin đơn hàng. Vui lòng thử lại sau."));
            }
        }

        //public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        //{
        //    var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

        //    return Redirect(url);
        //}


        [HttpPost("create-payment-url")]
        public IActionResult CreatePaymentUrl([FromBody] PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Ok(new { QrCodeUrl = url });
            return Redirect(url);
        }

        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            //return Json(response);

            return Ok(response);
        }



    }
}
