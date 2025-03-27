using ASM_C_5.Data;
using ASM_C_5.Models;
using ASM_C_5.DTOS.Responses;
using ASM_C_5.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace ASM_C_5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComboItemsController : ControllerBase
    {
        private readonly ASM_C_5Context _context;
        private readonly IWebHostEnvironment _env;

        public ComboItemsController(ASM_C_5Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<ComboItemsDTO>>>> GetAll()
        {
            var combos = await _context.ComboItems
                .Select(c => new ComboItemsDTO
                {
                    ComboID = c.ComboID,
                    ComboName = c.ComboName,
                    Price = c.Price,
                    Image = c.Image
                })
                .ToListAsync();

            return new BaseResponse<List<ComboItemsDTO>>
            {
                ErrorCode = 0,
                Message = "Lấy danh sách combo thành công",
                Data = combos
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<ComboItemsDTO>>> GetById(int id)
        {
            var combo = await _context.ComboItems
                .Where(c => c.ComboID == id)
                .Select(c => new ComboItemsDTO
                {
                    ComboID = c.ComboID,
                    ComboName = c.ComboName,
                    Price = c.Price,
                    Image = c.Image
                })
                .FirstOrDefaultAsync();

            if (combo == null)
            {
                return NotFound(new BaseResponse<ComboItemsDTO>
                {
                    ErrorCode = 404,
                    Message = "Combo không tồn tại",
                    Data = null
                });
            }

            return new BaseResponse<ComboItemsDTO>
            {
                ErrorCode = 0,
                Message = "Lấy thông tin combo thành công",
                Data = combo
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<ActionResult<BaseResponse<ComboItemsDTO>>> Create([FromForm] ComboItemsDTO comboItemDto, [FromForm] IFormFile? imageFile)
        {
            string imagePath = "/images/default.jpg";
            if (imageFile != null)
            {
                imagePath = await SaveImage(imageFile);
            }

            var comboItem = new ComboItem
            {
                ComboName = comboItemDto.ComboName,
                Price = comboItemDto.Price,
                Image = imagePath,
                CreatedBy = "admin",
                UpdatedBy = "admin"
            };

            await _context.AddAsync(comboItem);
            await _context.SaveChangesAsync();

            var responseDto = new ComboItemsDTO
            {
                ComboID = comboItem.ComboID,
                ComboName = comboItem.ComboName,
                Price = comboItem.Price,
                Image = comboItem.Image
            };

            return new BaseResponse<ComboItemsDTO>
            {
                ErrorCode = 0,
                Message = "Tạo combo thành công",
                Data = responseDto
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Update(int id, [FromForm] ComboItemsDTO comboItemDto, [FromForm] IFormFile? imageFile)
        {
            var existingCombo = await _context.ComboItems.FindAsync(id);
            if (existingCombo == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Combo không tồn tại",
                    Data = false
                });
            }

            existingCombo.ComboName = comboItemDto.ComboName;
            existingCombo.Price = comboItemDto.Price;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existingCombo.Image))
                {
                    DeleteImage(existingCombo.Image);
                }
                existingCombo.Image = await SaveImage(imageFile);
            }

            _context.ComboItems.Update(existingCombo);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Cập nhật combo thành công",
                Data = true
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var comboItem = await _context.ComboItems.FindAsync(id);
            if (comboItem == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Combo không tồn tại",
                    Data = false
                });
            }

            if (!string.IsNullOrEmpty(comboItem.Image))
            {
                DeleteImage(comboItem.Image);
            }

            _context.ComboItems.Remove(comboItem);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Xóa combo thành công",
                Data = true
            };
        }
        [Authorize(Roles = "Admin,Employee")]
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return "default.jpg"; // Trả về ảnh mặc định nếu không có ảnh

            // Định nghĩa thư mục lưu trữ ảnh trong wwwroot/public/images
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "public", "comboitems");

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
            return $"/public/comboitems/{fileName}";
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
