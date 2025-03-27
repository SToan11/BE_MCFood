using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.Models
{
    public class ComboDetail
    {
        [Required]
        public int FoodID { get; set; }

        [Required]
        public int ComboID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        // Khóa ngoại
        [ForeignKey("FoodID")]
        public FoodItem Food { get; set; }

        [ForeignKey("ComboID")]
        public ComboItem Combo { get; set; }
    }
}
