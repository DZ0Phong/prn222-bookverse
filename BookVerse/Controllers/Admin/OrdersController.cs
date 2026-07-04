using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class OrdersController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public OrdersController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Orders
        public async Task<IActionResult> Index(string? search, string? status, string? fromDate, string? toDate, int page = 1)
        {
            ViewData["Title"] = "Quản lý đơn hàng";
            ViewData["ActiveMenu"] = "Orders";

            const int pageSize = 15;

            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // Tab counts (before filtering)
            var countAll = await _context.Orders.CountAsync();
            var countPending = await _context.Orders.CountAsync(o => o.Status == "Chờ xử lý");
            var countShipping = await _context.Orders.CountAsync(o => o.Status == "Đang giao");
            var countDelivered = await _context.Orders.CountAsync(o => o.Status == "Đã giao");
            var countCancelled = await _context.Orders.CountAsync(o => o.Status == "Đã hủy");

            // Filter by status
            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int orderId))
                    query = query.Where(o => o.OrderId == orderId);
                else
                    query = query.Where(o => o.User != null && (o.User.FullName!.Contains(search) || o.User.Email!.Contains(search)));
            }

            // Date filter
            if (DateTime.TryParse(fromDate, out DateTime from))
                query = query.Where(o => o.CreatedAt >= from);

            if (DateTime.TryParse(toDate, out DateTime to))
                query = query.Where(o => o.CreatedAt <= to.AddDays(1));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderListItem
                {
                    OrderId = o.OrderId,
                    CustomerName = o.User != null ? o.User.FullName ?? o.User.Email : "N/A",
                    CustomerEmail = o.User != null ? o.User.Email : null,
                    CreatedAt = o.CreatedAt,
                    ItemCount = o.OrderDetails.Sum(d => d.Quantity ?? 0),
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod
                })
                .ToListAsync();

            var vm = new OrderListViewModel
            {
                Orders = orders,
                SearchTerm = search,
                StatusFilter = status,
                FromDate = fromDate,
                ToDate = toDate,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                CountAll = countAll,
                CountPending = countPending,
                CountShipping = countShipping,
                CountDelivered = countDelivered,
                CountCancelled = countCancelled
            };

            return View(vm);
        }

        // GET /Admin/Orders/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            ViewData["Title"] = $"Chi tiết đơn #{id}";
            ViewData["ActiveMenu"] = "Orders";

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new OrderDetailViewModel
            {
                OrderId = order.OrderId,
                CustomerName = order.User?.FullName ?? order.User?.Email ?? "N/A",
                CustomerEmail = order.User?.Email,
                CustomerPhone = order.User?.Phone,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                VoucherCode = order.Voucher?.Code,
                TotalAmount = order.TotalAmount ?? 0,
                Items = order.OrderDetails.Select(d => new OrderDetailItem
                {
                    BookId = d.BookId ?? 0,
                    BookTitle = d.Book?.Title ?? "N/A",
                    BookImage = d.Book?.Image,
                    BookAuthor = d.Book?.Author,
                    Quantity = d.Quantity ?? 0,
                    UnitPrice = d.Price ?? 0
                }).ToList()
            };

            return View(vm);
        }

        // POST /Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            var allowed = new[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" };
            if (!allowed.Contains(newStatus))
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });

            // Business logic: Can't go back from Delivered or Cancelled
            if (order.Status == "Đã giao" || order.Status == "Đã hủy")
                return Json(new { success = false, message = "Không thể thay đổi trạng thái đơn đã hoàn thành hoặc đã hủy" });

            order.Status = newStatus;

            // If order is cancelled, update payment status
            if (newStatus == "Đã hủy")
                order.PaymentStatus = "Đã hủy";

            // If delivered, mark as paid
            if (newStatus == "Đã giao" && order.PaymentMethod == "COD")
                order.PaymentStatus = "Đã thanh toán";

            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus, message = $"Đã cập nhật trạng thái thành \"{newStatus}\"" });
        }
    }
}
