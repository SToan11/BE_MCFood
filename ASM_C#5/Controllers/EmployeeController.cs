﻿using ASM_C_5.DTOS.Responses;
using ASM_C_5.DTOS;
using ASM_C_5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASM_C_5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // Thêm RoleManager

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var usersInAdminRole = await _userManager.GetUsersInRoleAsync("Employee");

            var users = usersInAdminRole.Select(user => new User_DTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address
            }).ToList();

            return Ok(new BaseResponse<List<User_DTO>>
            {
                ErrorCode = 200,
                Message = "Lấy danh sách nhân viên thành công!",
                Data = users
            });
        }
        [Authorize(Roles = "Admin, Employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra xem user có thuộc role "Admin" không
            var isAdmin = await _userManager.IsInRoleAsync(user, "Employee");
            if (!isAdmin)
            {
                return Forbid(); // Trả về lỗi 403 nếu user không phải admin
            }

            var userResponse = new User_DTO
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address
            };

            return Ok(new BaseResponse<User_DTO>
            {
                ErrorCode = 200,
                Message = "Lấy thông tin nhân viên thành công!",
                Data = userResponse
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Register_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Email đã tồn tại. Vui lòng sử dụng email khác.",
                    Data = null
                });
            }

            var existingUserByName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserByName != null)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.",
                    Data = null
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Address = request.Address
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // Đảm bảo role "Admin" tồn tại trước khi thêm user vào
                if (!await _roleManager.RoleExistsAsync("Employee"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employee"));
                }

                // Thêm người dùng vào role "Admin"
                await _userManager.AddToRoleAsync(user, "Employee");

                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Đăng ký Nhân viên thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Đăng ký thất bại",
                Data = result.Errors
            });
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUser_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value; // Lấy ID từ token
            if (userId == null)
            {
                return Unauthorized(new BaseResponse<string>
                {
                    ErrorCode = 401,
                    Message = "Không xác định được người dùng",
                    Data = null
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Cập nhật thông tin người dùng
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Address = request.Address ?? user.Address;
            user.Email = request.Email ?? user.Email;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Cập nhật thông tin thành công!",
                    Data = user.Id
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Cập nhật thất bại",
                Data = result.Errors
            });
        }
        [Authorize(Roles = "Admin,Employee")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword_DTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Dữ liệu không hợp lệ",
                    Data = null
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value; // Lấy ID từ token
            if (userId == null)
            {
                return Unauthorized(new BaseResponse<string>
                {
                    ErrorCode = 401,
                    Message = "Không xác định được người dùng",
                    Data = null
                });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new BaseResponse<string>
                {
                    ErrorCode = 404,
                    Message = "Người dùng không tồn tại",
                    Data = null
                });
            }

            // Kiểm tra mật khẩu cũ có đúng không
            var passwordCheck = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!passwordCheck)
            {
                return BadRequest(new BaseResponse<string>
                {
                    ErrorCode = 400,
                    Message = "Mật khẩu cũ không đúng",
                    Data = null
                });
            }

            // Thực hiện đổi mật khẩu
            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new BaseResponse<string>
                {
                    ErrorCode = 200,
                    Message = "Đổi mật khẩu thành công!",
                    Data = null
                });
            }

            return BadRequest(new BaseResponse<object>
            {
                ErrorCode = 400,
                Message = "Đổi mật khẩu thất bại",
                Data = result.Errors
            });
        }
    }
}
