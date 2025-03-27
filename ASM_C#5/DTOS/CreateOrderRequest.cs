using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.DTOS
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Items are required")]
        public List<OrderItemRequest> Items { get; set; }
    }

    public class OrderItemRequest
    {
        public int? FoodID { get; set; } // Có thể null nếu là combo
        public int? ComboID { get; set; } // Có thể null nếu là món lẻ

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

}
