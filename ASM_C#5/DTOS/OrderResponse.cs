//using static ASM_C_5.Controllers.CheckoutController;

namespace ASM_C_5.DTOS
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public CustomerInfo Customer { get; set; }
        public List<OrderProductInfo> Products { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
