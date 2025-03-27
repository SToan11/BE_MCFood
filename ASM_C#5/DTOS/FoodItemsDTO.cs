using ASM_C_5.Models;

namespace ASM_C_5.DTOS
{
    public class FoodItemsDTO
    {
        public int FoodID { get; set; }
        public string FoodName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public string? Image { get; set; }
    }
}
