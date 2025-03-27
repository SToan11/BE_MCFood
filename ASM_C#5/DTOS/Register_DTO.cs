using ASM_C_5.Models;
using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.DTOS
{
    public class Register_DTO : ModelBase
    {
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]

        public string ConfirmPassword { get; set; }
    }
}
