//using static ASM_C_5.Controllers.CheckoutController;

namespace ASM_C_5.DTOS
{
    public class OrderDetailsResponse
    {
        public int OrderId { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public CustomerInfo Customer { get; set; }
        public List<OrderProductInfo> Products { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
