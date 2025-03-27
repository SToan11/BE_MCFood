using ASM_C_5.Data;
using ASM_C_5.DTOS;
using ASM_C_5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASM_C_5.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ASM_C_5Context _context;

        public CartsController(ASM_C_5Context context)
        {
            _context = context;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetCart()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Get or create cart for user
        //    var cart = await GetOrCreateCartAsync(userId);

        //    // Include cart items in the response
        //    var cartWithItems = await _context.Carts
        //        .Include(c => c.CartItems)
        //        .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

        //    return Ok(cartWithItems);
        //}
        [HttpGet("items")]
        public async Task<IActionResult> GetAllCartItems()
        {
            try
            {
                var userId = GetUserId();

                // Lấy cart hiện tại của user
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return NotFound("Không tìm thấy giỏ hàng.");
                }

                // Lấy thông tin chi tiết của từng sản phẩm trong cart
                var cartItemsWithDetails = await _context.CartItems
                    .Where(ci => ci.CartId == cart.CartId)
                    .Select(ci => new
                    {
                        ci.CartItemId,
                        ci.ProductName,
                        ci.Quantity,
                        ci.Price,
                        TotalPrice = ci.Price * ci.Quantity,
                        ProductType = ci.FoodID.HasValue ? "Food" : "Combo",
                        ProductId = ci.FoodID ?? ci.ComboID,
                        // Thêm thông tin về Food hoặc Combo nếu cần
                        FoodDetails = ci.FoodID != null ? _context.FoodItems
                            .Where(f => f.FoodID == ci.FoodID)
                            .Select(f => new { f.FoodName, f.Description, f.Image })
                            .FirstOrDefault() : null,
                        ComboDetails = ci.ComboID != null ? _context.ComboItems
                            .Where(c => c.ComboID == ci.ComboID)
                            .FirstOrDefault() : null
                    })
                    .ToListAsync();

                var response = new
                {
                    CartId = cart.CartId,
                    TotalItems = cartItemsWithDetails.Count,
                    TotalAmount = cartItemsWithDetails.Sum(item => item.TotalPrice),
                    Items = cartItemsWithDetails
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin giỏ hàng", error = ex.Message });
            }
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.Sid);

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            return userId;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request data");

                if (request.FoodID == null && request.ComboID == null)
                    return BadRequest("Phải có ít nhất một trong FoodID hoặc ComboID.");

                var userId = GetUserId();

                // Get or create cart
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Get product details
                string productName = "";
                decimal price = 0;

                if (request.FoodID.HasValue)
                {
                    var food = await _context.FoodItems.FindAsync(request.FoodID.Value);
                    if (food == null) return NotFound("Món ăn không tồn tại.");
                    productName = food.FoodName;
                    price = food.Price;
                }
                else if (request.ComboID.HasValue)
                {
                    var combo = await _context.ComboItems.FindAsync(request.ComboID.Value);
                    if (combo == null) return NotFound("Combo không tồn tại.");
                    productName = combo.ComboName;
                    price = combo.Price;
                }

                // Check for existing cart item
                var cartItem = await _context.CartItems  // Changed from CartItem to CartItems
                    .FirstOrDefaultAsync(ci =>
                        ci.CartId == cart.CartId &&
                        ci.FoodID == request.FoodID &&
                        ci.ComboID == request.ComboID);

                if (cartItem == null)
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        FoodID = request.FoodID,
                        ComboID = request.ComboID,
                        ProductName = productName,
                        Price = price,
                        Quantity = request.Quantity
                    };
                    _context.CartItems.Add(cartItem);  // Changed from CartItem to CartItems
                }
                else
                {
                    cartItem.Quantity += request.Quantity;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đã thêm vào giỏ hàng.",
                    cartItemId = cartItem.CartItemId,
                    cartId = cart.CartId
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User ID not found in token");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var cartItem = await _context.CartItems  // Changed from CartItem to CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.Cart.UserId == userId &&
                    ci.CartItemId == cartItemId);

            if (cartItem == null)
                return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

            _context.CartItems.Remove(cartItem);  // Changed from CartItem to CartItems
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng." });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return NotFound("Giỏ hàng trống.");

            _context.CartItems.RemoveRange(cart.CartItems);  // Changed from CartItem to CartItems
            await _context.SaveChangesAsync();
            return Ok("Đã xóa toàn bộ giỏ hàng.");
        }

        // Helper methods
        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<(string productName, decimal price)> GetProductDetailsAsync(CartItemDTO request)
        {
            if (request.FoodID.HasValue)
            {
                var food = await _context.FoodItems.FindAsync(request.FoodID.Value);
                if (food != null)
                    return (food.FoodName, food.Price);
            }
            else if (request.ComboID.HasValue)
            {
                var combo = await _context.ComboItems.FindAsync(request.ComboID.Value);
                if (combo != null)
                    return (combo.ComboName, combo.Price);
            }

            return (null, 0);
        }

        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateCart(int cartItemId, [FromBody] UpdateCartItemDTO request)
        {
            try
            {
                if (request == null || request.Quantity <= 0)
                    return BadRequest("Số lượng sản phẩm phải lớn hơn 0.");

                var userId = GetUserId();

                // Tìm sản phẩm trong giỏ hàng
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.CartItemId == cartItemId);

                if (cartItem == null)
                    return NotFound("Sản phẩm không tồn tại trong giỏ hàng.");

                // Cập nhật số lượng sản phẩm
                cartItem.Quantity = request.Quantity;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật giỏ hàng thành công.",
                    cartItemId = cartItem.CartItemId,
                    newQuantity = cartItem.Quantity
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User ID not found in token");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật giỏ hàng", error = ex.Message });
            }
        }
        public class UpdateCartItemDTO
        {
            public int Quantity { get; set; }
        }
    }
}
