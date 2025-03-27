using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.Models
{
    public class FoodCategory : ModelBase
    {
        [Key]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Tên Loại Không Được Để Trống")]

        public string CategoryName { get; set; }
    }
}
