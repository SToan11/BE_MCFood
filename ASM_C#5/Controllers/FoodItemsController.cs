using ASM_C_5.Data;
using ASM_C_5.Models;
using ASM_C_5.DTOS.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using ASM_C_5.DTOS;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace ASM_C_5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly ASM_C_5Context _context;
        private readonly IWebHostEnvironment _env;

        public FoodItemsController(ASM_C_5Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<FoodItemsDTO>>>> GetAll()
        {
            var foodItems = await _context.FoodItems
                .Select(f => new FoodItemsDTO
                {
                    FoodID = f.FoodID,
                    FoodName = f.FoodName,
                    Price = f.Price,
                    Description = f.Description,
                    CategoryID = f.CategoryID,
                    Image = f.Image
                })
                .ToListAsync();

            return new BaseResponse<List<FoodItemsDTO>>
            {
                ErrorCode = 0,
                Message = "Lấy danh sách món ăn thành công",
                Data = foodItems
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<FoodItemsDTO>>> GetById(int id)
        {
            var foodItem = await _context.FoodItems
                .Where(f => f.FoodID == id)
                .Select(f => new FoodItemsDTO
                {
                    FoodID = f.FoodID,
                    FoodName = f.FoodName,
                    Price = f.Price,
                    Description = f.Description,
                    CategoryID = f.CategoryID,
                    Image = f.Image
                })
                .FirstOrDefaultAsync();

            if (foodItem == null)
            {
                return NotFound(new BaseResponse<FoodItemsDTO>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại",
                    Data = null
                });
            }

            return new BaseResponse<FoodItemsDTO>
            {
                ErrorCode = 0,
                Message = "Lấy thông tin món ăn thành công",
                Data = foodItem
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<FoodItemsDTO>>> Create([FromForm] FoodItemsDTO foodItem, [FromForm] IFormFile? imageFile)
        {
            if (string.IsNullOrEmpty(foodItem.FoodName) || foodItem.Price <= 0)
            {
                return BadRequest(new BaseResponse<FoodItemsDTO>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            string imagePath = "/images/default.jpg";
            if (imageFile != null && imageFile.Length > 0)
            {
                imagePath = await SaveImage(imageFile);
            }

            var newFood = new FoodItem
            {
                FoodName = foodItem.FoodName,
                Price = foodItem.Price,
                Description = foodItem.Description,
                CategoryID = foodItem.CategoryID,
                Image = imagePath,
                CreatedBy = "admin",
                UpdatedBy = "admin"
            };

            await _context.FoodItems.AddAsync(newFood);
            await _context.SaveChangesAsync();

            return new BaseResponse<FoodItemsDTO>
            {
                ErrorCode = 0,
                Message = "Thêm món ăn thành công",
                Data = new FoodItemsDTO
                {
                    FoodID = newFood.FoodID,
                    FoodName = newFood.FoodName,
                    Price = newFood.Price,
                    Description = newFood.Description,
                    CategoryID = newFood.CategoryID,
                    Image = newFood.Image,
                }
            };
        }


        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Update(int id, [FromForm] FoodItemsDTO foodItem, [FromForm] IFormFile? imageFile)
        {
            var existingFood = await _context.FoodItems.FindAsync(id);
            if (existingFood == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại",
                    Data = false
                });
            }

            if (string.IsNullOrEmpty(foodItem.FoodName) || foodItem.Price <= 0)
            {
                return BadRequest(new BaseResponse<bool>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = false
                });
            }

            existingFood.FoodName = foodItem.FoodName;
            existingFood.Price = foodItem.Price;
            existingFood.Description = foodItem.Description;
            existingFood.CategoryID = foodItem.CategoryID;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existingFood.Image) && existingFood.Image != "/images/default.jpg")
                {
                    DeleteImage(existingFood.Image);
                }
                existingFood.Image = await SaveImage(imageFile);
            }

            _context.FoodItems.Update(existingFood);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Cập nhật thành công",
                Data = true
            };
        }


        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            // Tìm món ăn trong cơ sở dữ liệu
            var food = await _context.FoodItems.FindAsync(id);
            if (food == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Món ăn không tồn tại",
                    Data = false
                });
            }

            // Kiểm tra nếu có hình ảnh và xóa nếu có
            if (!string.IsNullOrEmpty(food.Image))
            {
                DeleteImage(food.Image);  // Xóa ảnh liên quan
            }

            // Xóa món ăn khỏi cơ sở dữ liệu
            _context.FoodItems.Remove(food);
            await _context.SaveChangesAsync();

            // Trả về phản hồi sau khi xóa thành công
            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Xóa món ăn thành công",
                Data = true
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return "default.jpg"; // Trả về ảnh mặc định nếu không có ảnh

            // Định nghĩa thư mục lưu trữ ảnh trong wwwroot/public/images
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "public", "fooditems");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder); // Tạo thư mục nếu chưa có

            // Tạo tên file ngẫu nhiên để tránh trùng lặp
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file vào thư mục đã chỉ định
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Trả về đường dẫn ảnh để lưu vào database (dùng URL tương đối cho frontend React)
            return $"/public/fooditems/{fileName}";
        }


        [Authorize(Roles = "Admin,Employee")]
        private void DeleteImage(string imagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath)) return;

                var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace("/", "\\"));

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
            }
        }
    }
}
