using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.Models
{
    public class ComboItem : ModelBase
    {
        [Key]
        public int ComboID { get; set; }
        [Required(ErrorMessage = "Tên không được để trống")]
        public string ComboName { get; set; }
        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        public string? Image { get; set; }
    }
}
