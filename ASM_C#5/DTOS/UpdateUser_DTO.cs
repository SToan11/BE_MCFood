using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.DTOS
{
    public class UpdateUser_DTO
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

        [Required(ErrorMessage = "Địa chỉ không được để trống.")]
        public string Address { get; set; }
    }
}
