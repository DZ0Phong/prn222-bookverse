using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class DashboardController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public DashboardController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["ActiveMenu"] = "Dashboard";

            var now = DateTime.Now;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddSeconds(-1);
            var thirtyDaysAgo = now.AddDays(-30).Date;

            var vm = new DashboardViewModel();

            // ===== Stat Cards =====

            // Tổng doanh thu tháng này
            var thisMonthOrders = await _context.Orders
                .Where(o => o.CreatedAt >= thisMonthStart && o.Status != "Đã hủy")
                .ToListAsync();
            vm.TotalRevenue = thisMonthOrders.Sum(o => o.TotalAmount ?? 0);
            vm.TotalOrders = thisMonthOrders.Count;

            // Sách đã bán tháng này
            var thisMonthOrderIds = thisMonthOrders.Select(o => o.OrderId).ToList();
            var thisMonthDetails = await _context.OrderDetails
                .Where(d => thisMonthOrderIds.Contains(d.OrderId ?? 0))
                .ToListAsync();
            vm.TotalBooksSold = thisMonthDetails.Sum(d => d.Quantity ?? 0);

            // Người dùng mới tháng này
            vm.NewUsers = await _context.Users
                .CountAsync(u => u.CreatedAt >= thisMonthStart);

            // Last month stats for comparison
            var lastMonthOrders = await _context.Orders
                .Where(o => o.CreatedAt >= lastMonthStart && o.CreatedAt <= lastMonthEnd && o.Status != "Đã hủy")
                .ToListAsync();
            var lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalAmount ?? 0);
            var lastMonthCount = lastMonthOrders.Count;

            var lastMonthOrderIds = lastMonthOrders.Select(o => o.OrderId).ToList();
            var lastMonthDetails = await _context.OrderDetails
                .Where(d => lastMonthOrderIds.Contains(d.OrderId ?? 0))
                .ToListAsync();
            var lastMonthBooks = lastMonthDetails.Sum(d => d.Quantity ?? 0);

            var lastMonthUsers = await _context.Users
                .CountAsync(u => u.CreatedAt >= lastMonthStart && u.CreatedAt <= lastMonthEnd);

            vm.RevenueChangePercent = lastMonthRevenue == 0 ? 100 : (double)((vm.TotalRevenue - lastMonthRevenue) / lastMonthRevenue * 100);
            vm.OrdersChangePercent = lastMonthCount == 0 ? 100 : (double)((vm.TotalOrders - lastMonthCount) / (double)lastMonthCount * 100);
            vm.BooksSoldChangePercent = lastMonthBooks == 0 ? 100 : (double)((vm.TotalBooksSold - lastMonthBooks) / (double)lastMonthBooks * 100);
            vm.UsersChangePercent = lastMonthUsers == 0 ? 100 : (double)((vm.NewUsers - lastMonthUsers) / (double)lastMonthUsers * 100);

            // ===== Revenue Chart (Last 30 Days) =====
            var revenueByDay = await _context.Orders
                .Where(o => o.CreatedAt >= thirtyDaysAgo && o.Status != "Đã hủy")
                .GroupBy(o => o.CreatedAt!.Value.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount ?? 0) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Fill all 30 days (even if 0)
            for (int i = 29; i >= 0; i--)
            {
                var date = now.AddDays(-i).Date;
                vm.RevenueDates.Add(date.ToString("dd/MM"));
                var dayRevenue = revenueByDay.FirstOrDefault(x => x.Date == date);
                vm.RevenueAmounts.Add(dayRevenue?.Revenue ?? 0);
            }

            // ===== Top 5 Best-Selling Books =====
            var topBooks = await _context.OrderDetails
                .Include(d => d.Book)
                .Include(d => d.Order)
                .Where(d => d.Order != null && d.Order.Status != "Đã hủy")
                .GroupBy(d => new { d.BookId, d.Book!.Title, d.Book.Author, d.Book.Image })
                .Select(g => new TopBookItem
                {
                    BookId = g.Key.BookId ?? 0,
                    Title = g.Key.Title ?? "N/A",
                    Author = g.Key.Author,
                    Image = g.Key.Image,
                    TotalSold = g.Sum(d => d.Quantity ?? 0),
                    TotalRevenue = g.Sum(d => (d.Price ?? 0) * (d.Quantity ?? 0))
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();
            vm.TopBooks = topBooks;

            // ===== Revenue by Category (Pie Chart) =====
            var categoryRevenue = await _context.OrderDetails
                .Include(d => d.Book).ThenInclude(b => b!.Category)
                .Include(d => d.Order)
                .Where(d => d.Order != null && d.Order.Status != "Đã hủy")
                .GroupBy(d => d.Book!.Category!.CategoryName)
                .Select(g => new
                {
                    Category = g.Key ?? "Khác",
                    Revenue = g.Sum(d => (d.Price ?? 0) * (d.Quantity ?? 0))
                })
                .OrderByDescending(x => x.Revenue)
                .Take(6)
                .ToListAsync();

            vm.CategoryNames = categoryRevenue.Select(x => x.Category).ToList();
            vm.CategoryRevenues = categoryRevenue.Select(x => x.Revenue).ToList();

            // ===== Recent Orders =====
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderItem
                {
                    OrderId = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName ?? o.User.Email ?? "N/A" : "N/A",
                    CreatedAt = o.CreatedAt,
                    ItemCount = o.OrderDetails.Sum(d => d.Quantity ?? 0),
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus
                })
                .ToListAsync();
            vm.RecentOrders = recentOrders;

            return View(vm);
        }
    }
}
