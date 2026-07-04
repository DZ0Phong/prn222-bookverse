namespace BookVerse.Models.ViewModels.Admin
{
    public class DashboardViewModel
    {
        // Stat Cards - Current Month
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalBooksSold { get; set; }
        public int NewUsers { get; set; }

        // % change vs last month
        public double RevenueChangePercent { get; set; }
        public double OrdersChangePercent { get; set; }
        public double BooksSoldChangePercent { get; set; }
        public double UsersChangePercent { get; set; }

        // Revenue chart - last 30 days
        public List<string> RevenueDates { get; set; } = new();
        public List<decimal> RevenueAmounts { get; set; } = new();

        // Top 5 best-selling books
        public List<TopBookItem> TopBooks { get; set; } = new();

        // Revenue by category (pie chart)
        public List<string> CategoryNames { get; set; } = new();
        public List<decimal> CategoryRevenues { get; set; } = new();

        // Recent 10 orders
        public List<RecentOrderItem> RecentOrders { get; set; } = new();
    }

    public class TopBookItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? Image { get; set; }
    }

    public class RecentOrderItem
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
