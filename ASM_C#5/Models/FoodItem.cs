using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_C_5.Models
{
    public class FoodItem : ModelBase
    {
        [Key]
        public int FoodID { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        public string FoodName { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Phải chọn loại món ăn")]
        public int CategoryID { get; set; }

        public string? Image { get; set; } // Lưu đường dẫn ảnh (vd: /images/abc123.jpg)


        [ForeignKey("CategoryID")]
        public FoodCategory Category { get; set; }
    }
}
