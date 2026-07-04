using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class ReportsController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public ReportsController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Reports
        public async Task<IActionResult> Index(string? fromDate, string? toDate, string groupBy = "day")
        {
            ViewData["Title"] = "Thống kê & Báo cáo";
            ViewData["ActiveMenu"] = "Reports";

            // Default: this month
            var from = DateTime.TryParse(fromDate, out var f) ? f : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = DateTime.TryParse(toDate, out var t) ? t : DateTime.Now;

            var vm = new ReportViewModel
            {
                FromDate = from.ToString("yyyy-MM-dd"),
                ToDate = to.ToString("yyyy-MM-dd"),
                GroupBy = groupBy
            };

            var toInclusive = to.Date.AddDays(1);

            // Orders in range (not cancelled)
            var ordersInRange = await _context.Orders
                .Where(o => o.CreatedAt >= from && o.CreatedAt < toInclusive && o.Status != "Đã hủy")
                .ToListAsync();

            vm.TotalRevenue = ordersInRange.Sum(o => o.TotalAmount ?? 0);
            vm.TotalOrders = ordersInRange.Count;
            vm.AverageOrderValue = ordersInRange.Count > 0 ? vm.TotalRevenue / ordersInRange.Count : 0;

            // Total books sold
            var orderIds = ordersInRange.Select(o => o.OrderId).ToList();
            var details = await _context.OrderDetails
                .Where(d => orderIds.Contains(d.OrderId ?? 0))
                .ToListAsync();
            vm.TotalBooksSold = details.Sum(d => d.Quantity ?? 0);

            // Revenue chart data
            await BuildChartData(vm, from, to, groupBy, ordersInRange);

            // Top 10 Best-Selling Books
            vm.TopBooks = await _context.OrderDetails
                .Include(d => d.Book).ThenInclude(b => b!.Category)
                .Include(d => d.Order)
                .Where(d => d.Order != null && d.Order.CreatedAt >= from && d.Order.CreatedAt < toInclusive && d.Order.Status != "Đã hủy")
                .GroupBy(d => new { d.BookId, d.Book!.Title, d.Book.Author, CategoryName = d.Book.Category != null ? d.Book.Category.CategoryName : "Khác" })
                .Select(g => new TopBookReportItem
                {
                    BookId = g.Key.BookId ?? 0,
                    Title = g.Key.Title ?? "N/A",
                    Author = g.Key.Author,
                    CategoryName = g.Key.CategoryName,
                    TotalSold = g.Sum(d => d.Quantity ?? 0),
                    TotalRevenue = g.Sum(d => (d.Price ?? 0) * (d.Quantity ?? 0))
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();

            // Top Customers
            vm.TopCustomers = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.CreatedAt >= from && o.CreatedAt < toInclusive && o.Status != "Đã hủy" && o.User != null)
                .GroupBy(o => new { o.UserId, Name = o.User!.FullName ?? o.User.Email, o.User.Email })
                .Select(g => new TopCustomerItem
                {
                    UserId = g.Key.UserId ?? 0,
                    Name = g.Key.Name ?? "N/A",
                    Email = g.Key.Email,
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.TotalAmount ?? 0)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToListAsync();

            return View(vm);
        }

        // GET /Admin/Reports/ExportExcel
        public async Task<IActionResult> ExportExcel(string? fromDate, string? toDate)
        {
            var from = DateTime.TryParse(fromDate, out var f) ? f : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = DateTime.TryParse(toDate, out var t) ? t : DateTime.Now;
            var toInclusive = to.Date.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Book)
                .Include(o => o.Voucher)
                .Where(o => o.CreatedAt >= from && o.CreatedAt < toInclusive)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            using var workbook = new XLWorkbook();

            // === Sheet 1: Orders Summary ===
            var ws1 = workbook.Worksheets.Add("Danh sách đơn hàng");

            // Header
            ws1.Cell(1, 1).Value = $"BÁO CÁO ĐƠN HÀNG: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
            ws1.Range(1, 1, 1, 8).Merge();
            ws1.Cell(1, 1).Style.Font.Bold = true;
            ws1.Cell(1, 1).Style.Font.FontSize = 14;
            ws1.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#e94560");

            var headers = new[] { "Mã đơn", "Khách hàng", "Email", "Ngày đặt", "Tổng tiền (₫)", "Voucher", "Trạng thái", "Thanh toán" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws1.Cell(3, i + 1).Value = headers[i];
                ws1.Cell(3, i + 1).Style.Font.Bold = true;
                ws1.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
                ws1.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
                ws1.Cell(3, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 4;
            foreach (var order in orders)
            {
                ws1.Cell(row, 1).Value = $"#" + order.OrderId;
                ws1.Cell(row, 2).Value = order.User?.FullName ?? "N/A";
                ws1.Cell(row, 3).Value = order.User?.Email ?? "";
                ws1.Cell(row, 4).Value = order.CreatedAt?.ToString("dd/MM/yyyy HH:mm");
                ws1.Cell(row, 5).Value = (double)(order.TotalAmount ?? 0);
                ws1.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                ws1.Cell(row, 6).Value = order.Voucher?.Code ?? "";
                ws1.Cell(row, 7).Value = order.Status;
                ws1.Cell(row, 8).Value = order.PaymentStatus;

                if (order.Status == "Đã hủy")
                    ws1.Row(row).Style.Font.FontColor = XLColor.Red;
                else if (order.Status == "Đã giao")
                    ws1.Row(row).Style.Font.FontColor = XLColor.DarkGreen;

                row++;
            }

            // Summary row
            ws1.Cell(row + 1, 4).Value = "TỔNG CỘNG:";
            ws1.Cell(row + 1, 4).Style.Font.Bold = true;
            var totalRevenue = orders.Where(o => o.Status != "Đã hủy").Sum(o => (double)(o.TotalAmount ?? 0));
            ws1.Cell(row + 1, 5).Value = totalRevenue;
            ws1.Cell(row + 1, 5).Style.NumberFormat.Format = "#,##0";
            ws1.Cell(row + 1, 5).Style.Font.Bold = true;

            ws1.Columns().AdjustToContents();

            // === Sheet 2: Top Books ===
            var ws2 = workbook.Worksheets.Add("Top sách bán chạy");
            ws2.Cell(1, 1).Value = "TOP SÁCH BÁN CHẠY";
            ws2.Range(1, 1, 1, 5).Merge();
            ws2.Cell(1, 1).Style.Font.Bold = true;
            ws2.Cell(1, 1).Style.Font.FontSize = 13;

            var bookHeaders = new[] { "Tên sách", "Tác giả", "Danh mục", "SL bán", "Doanh thu (₫)" };
            for (int i = 0; i < bookHeaders.Length; i++)
            {
                ws2.Cell(3, i + 1).Value = bookHeaders[i];
                ws2.Cell(3, i + 1).Style.Font.Bold = true;
                ws2.Cell(3, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
                ws2.Cell(3, i + 1).Style.Font.FontColor = XLColor.White;
            }

            var topBooks = await _context.OrderDetails
                .Include(d => d.Book).ThenInclude(b => b!.Category)
                .Include(d => d.Order)
                .Where(d => d.Order != null && d.Order.CreatedAt >= from && d.Order.CreatedAt < toInclusive && d.Order.Status != "Đã hủy")
                .GroupBy(d => new { d.Book!.Title, d.Book.Author, CatName = d.Book.Category != null ? d.Book.Category.CategoryName : "Khác" })
                .Select(g => new { g.Key.Title, g.Key.Author, g.Key.CatName, Sold = g.Sum(d => d.Quantity ?? 0), Rev = g.Sum(d => (d.Price ?? 0) * (d.Quantity ?? 0)) })
                .OrderByDescending(x => x.Sold)
                .Take(10)
                .ToListAsync();

            int bRow = 4;
            foreach (var b in topBooks)
            {
                ws2.Cell(bRow, 1).Value = b.Title;
                ws2.Cell(bRow, 2).Value = b.Author;
                ws2.Cell(bRow, 3).Value = b.CatName;
                ws2.Cell(bRow, 4).Value = b.Sold;
                ws2.Cell(bRow, 5).Value = (double)b.Rev;
                ws2.Cell(bRow, 5).Style.NumberFormat.Format = "#,##0";
                bRow++;
            }
            ws2.Columns().AdjustToContents();

            // Save to memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"BookVerse_Report_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // Private helper: build chart data
        private async Task BuildChartData(ReportViewModel vm, DateTime from, DateTime to, string groupBy, List<Order> orders)
        {
            if (groupBy == "month")
            {
                var grouped = orders
                    .GroupBy(o => new { o.CreatedAt!.Value.Year, o.CreatedAt.Value.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

                foreach (var g in grouped)
                {
                    vm.ChartLabels.Add($"{g.Key.Month:D2}/{g.Key.Year}");
                    vm.ChartRevenues.Add(g.Sum(o => o.TotalAmount ?? 0));
                }
            }
            else if (groupBy == "week")
            {
                var grouped = orders
                    .GroupBy(o => System.Globalization.ISOWeek.GetWeekOfYear(o.CreatedAt!.Value))
                    .OrderBy(g => g.Key);

                foreach (var g in grouped)
                {
                    vm.ChartLabels.Add($"Tuần {g.Key}");
                    vm.ChartRevenues.Add(g.Sum(o => o.TotalAmount ?? 0));
                }
            }
            else // day
            {
                // Fill each day in range
                for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
                {
                    vm.ChartLabels.Add(d.ToString("dd/MM"));
                    vm.ChartRevenues.Add(orders.Where(o => o.CreatedAt?.Date == d).Sum(o => o.TotalAmount ?? 0));
                }
            }

            await Task.CompletedTask;
        }
    }
}
