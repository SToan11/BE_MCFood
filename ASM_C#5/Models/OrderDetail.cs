using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM_C_5.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required]
        public int OrderID { get; set; }

        [ForeignKey("OrderID")]
        public Order Order { get; set; }

        public int? FoodID { get; set; }
        [ForeignKey("FoodID")]
        public FoodItem? Food { get; set; }

        public int? ComboID { get; set; }
        [ForeignKey("ComboID")]
        public ComboItem? Combo { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public string ProductName { get; set; }

        public bool IsValid() => FoodID.HasValue || ComboID.HasValue;
    }

}
