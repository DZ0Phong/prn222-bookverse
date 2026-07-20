namespace BookVerse.Models.ViewModels.User
{
    public class UserOrderListViewModel
    {
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
    }

    // Dùng cho trang xem chi tiết 1 đơn hàng cụ thể
    public class UserOrderDetailViewModel
    {
        public int OrderId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public List<UserOrderItemViewModel> Items { get; set; } = new();
    }

    // Dùng cho từng item cuốn sách trong đơn hàng
    public class UserOrderItemViewModel
    {
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? BookImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
