using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.Models
{
    public class Order : ModelBase
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; } // Mỗi đơn hàng thuộc về một người dùng

        [Required]
        public int CartId { get; set; } // Thêm CartId để biết đơn hàng này được tạo từ giỏ hàng nào

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Trạng thái: Pending, Completed

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
