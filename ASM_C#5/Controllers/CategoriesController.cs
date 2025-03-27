using ASM_C_5.Data;
using ASM_C_5.Models;
using ASM_C_5.DTOS;
using ASM_C_5.DTOS.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ASM_C_5.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ASM_C_5Context _context;

        public CategoriesController(ASM_C_5Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<List<CategorisDTO>>>> GetAll()
        {
            var categories = await _context.FoodCategories
                .Select(c => new CategorisDTO
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            return new BaseResponse<List<CategorisDTO>>
            {
                ErrorCode = 0,
                Message = "Lấy danh sách danh mục thành công",
                Data = categories
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<CategorisDTO>>> GetById(int id)
        {
            var category = await _context.FoodCategories
                .Where(c => c.CategoryID == id)
                .Select(c => new CategorisDTO
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new BaseResponse<CategorisDTO>
                {
                    ErrorCode = 404,
                    Message = "Danh mục không tồn tại",
                    Data = null
                });
            }

            return new BaseResponse<CategorisDTO>
            {
                ErrorCode = 0,
                Message = "Lấy thông tin danh mục thành công",
                Data = category
            };
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse<CategorisDTO>>> Create([FromBody] CategorisDTO category)
        {
            var newCategory = new FoodCategory
            {
                CategoryName = category.CategoryName
            };

            await _context.FoodCategories.AddAsync(newCategory);
            await _context.SaveChangesAsync();

            var categoryDTO = new CategorisDTO
            {
                CategoryID = newCategory.CategoryID,
                CategoryName = newCategory.CategoryName
            };

            return new BaseResponse<CategorisDTO>
            {
                ErrorCode = 0,
                Message = "Thêm danh mục thành công",
                Data = categoryDTO
            };
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Update(int id, [FromBody] CategorisDTO category)
        {
            var existingCategory = await _context.FoodCategories.FindAsync(id);
            if (existingCategory == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Danh mục không tồn tại",
                    Data = false
                });
            }

            existingCategory.CategoryName = category.CategoryName;

            _context.FoodCategories.Update(existingCategory);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Cập nhật danh mục thành công",
                Data = true
            };
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BaseResponse<bool>>> Delete(int id)
        {
            var category = await _context.FoodCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound(new BaseResponse<bool>
                {
                    ErrorCode = 404,
                    Message = "Danh mục không tồn tại",
                    Data = false
                });
            }

            _context.FoodCategories.Remove(category);
            await _context.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                ErrorCode = 0,
                Message = "Xóa danh mục thành công",
                Data = true
            };
        }
    }
}
