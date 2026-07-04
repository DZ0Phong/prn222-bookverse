namespace BookVerse.Models.ViewModels.Admin
{
    public class ReportViewModel
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string GroupBy { get; set; } = "day"; // day | week | month

        // Summary Cards
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalBooksSold { get; set; }

        // Revenue Chart Data
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartRevenues { get; set; } = new();

        // Top 10 Books
        public List<TopBookReportItem> TopBooks { get; set; } = new();

        // Top Customers
        public List<TopCustomerItem> TopCustomers { get; set; } = new();
    }

    public class TopBookReportItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? CategoryName { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopCustomerItem
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
