using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class VouchersController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public VouchersController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Vouchers
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý voucher";
            ViewData["ActiveMenu"] = "Vouchers";

            var vouchers = await _context.Vouchers
                .Include(v => v.Orders)
                .Select(v => new VoucherListItem
                {
                    VoucherId = v.VoucherId,
                    Code = v.Code,
                    DiscountPercent = v.DiscountPercent,
                    ExpiryDate = v.ExpiryDate,
                    IsActive = v.IsActive,
                    UsedCount = v.Orders.Count
                })
                .OrderByDescending(v => v.VoucherId)
                .ToListAsync();

            var vm = new VoucherListViewModel
            {
                Vouchers = vouchers,
                Form = new VoucherFormViewModel()
            };

            return View(vm);
        }

        // POST /Admin/Vouchers/Create (AJAX)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VoucherFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            if (form.ExpiryDate <= DateTime.Now)
                return Json(new { success = false, message = "Ngày hết hạn phải sau ngày hiện tại!" });

            // Check duplicate code
            var code = form.Code.Trim().ToUpper();
            if (await _context.Vouchers.AnyAsync(v => v.Code == code))
                return Json(new { success = false, message = $"Mã voucher \"{code}\" đã tồn tại!" });

            var voucher = new Voucher
            {
                Code = code,
                DiscountPercent = form.DiscountPercent,
                ExpiryDate = form.ExpiryDate,
                IsActive = form.IsActive
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã tạo voucher \"{voucher.Code}\" thành công!",
                data = new
                {
                    voucher.VoucherId,
                    voucher.Code,
                    voucher.DiscountPercent,
                    expiryDate = voucher.ExpiryDate?.ToString("dd/MM/yyyy"),
                    voucher.IsActive,
                    usedCount = 0
                }
            });
        }

        // POST /Admin/Vouchers/Edit (AJAX)
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] VoucherFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            var voucher = await _context.Vouchers.FindAsync(form.VoucherId);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher!" });

            if (form.ExpiryDate <= DateTime.Now)
                return Json(new { success = false, message = "Ngày hết hạn phải sau ngày hiện tại!" });

            var code = form.Code.Trim().ToUpper();
            if (await _context.Vouchers.AnyAsync(v => v.Code == code && v.VoucherId != form.VoucherId))
                return Json(new { success = false, message = $"Mã voucher \"{code}\" đã tồn tại!" });

            voucher.Code = code;
            voucher.DiscountPercent = form.DiscountPercent;
            voucher.ExpiryDate = form.ExpiryDate;
            voucher.IsActive = form.IsActive;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật voucher thành công!",
                data = new
                {
                    voucher.VoucherId,
                    voucher.Code,
                    voucher.DiscountPercent,
                    expiryDate = voucher.ExpiryDate?.ToString("dd/MM/yyyy"),
                    voucher.IsActive
                }
            });
        }

        // POST /Admin/Vouchers/Delete (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _context.Vouchers
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(v => v.VoucherId == id);

            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher!" });

            if (voucher.Orders.Any())
            {
                // Soft delete — deactivate
                voucher.IsActive = false;
                await _context.SaveChangesAsync();
                return Json(new { success = true, softDelete = true, message = $"Voucher \"{voucher.Code}\" đã được sử dụng nên chỉ bị vô hiệu hóa thay vì xóa!" });
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            return Json(new { success = true, softDelete = false, message = $"Đã xóa voucher \"{voucher.Code}\"!" });
        }

        // GET /Admin/Vouchers/GetById/{id} (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new
                {
                    voucher.VoucherId,
                    voucher.Code,
                    voucher.DiscountPercent,
                    expiryDate = voucher.ExpiryDate?.ToString("yyyy-MM-dd"),
                    voucher.IsActive
                }
            });
        }

        // POST /Admin/Vouchers/ToggleActive (AJAX)
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher" });

            voucher.IsActive = !(voucher.IsActive ?? true);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = voucher.IsActive, message = voucher.IsActive == true ? "Đã kích hoạt" : "Đã vô hiệu hóa" });
        }
    }
}
