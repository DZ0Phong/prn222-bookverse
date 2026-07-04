namespace BookVerse.Models.ViewModels.Admin
{
    public class OrderListViewModel
    {
        public List<OrderListItem> Orders { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 15;

        // Tab counts
        public int CountAll { get; set; }
        public int CountPending { get; set; }
        public int CountShipping { get; set; }
        public int CountDelivered { get; set; }
        public int CountCancelled { get; set; }
    }

    public class OrderListItem
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public string? VoucherCode { get; set; }
        public decimal TotalAmount { get; set; }

        public List<OrderDetailItem> Items { get; set; } = new();

        public List<string> AllowedStatuses => Status switch
        {
            "Chờ xử lý" => new List<string> { "Chờ xử lý", "Đang giao", "Đã hủy" },
            "Đang giao" => new List<string> { "Đang giao", "Đã giao", "Đã hủy" },
            "Đã giao" => new List<string> { "Đã giao" },
            "Đã hủy" => new List<string> { "Đã hủy" },
            _ => new List<string> { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" }
        };
    }

    public class OrderDetailItem
    {
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? BookImage { get; set; }
        public string? BookAuthor { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
