namespace ASM_C_5.DTOS
{
    public class OrderProductInfo
    {
        public string ProductName { get; set; }
        public int? ComboId { get; set; }
        public int? FoodId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
