using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class ReviewsController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public ReviewsController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Reviews
        public async Task<IActionResult> Index(bool? isApproved, int? rating, int page = 1)
        {
            ViewData["Title"] = "Quản lý đánh giá";
            ViewData["ActiveMenu"] = "Reviews";

            const int pageSize = 15;

            var query = _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsQueryable();

            if (isApproved.HasValue)
                query = query.Where(r => r.IsApproved == isApproved);

            if (rating.HasValue)
                query = query.Where(r => r.Rating == rating);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.IsApproved = isApproved;
            ViewBag.Rating = rating;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(reviews);
        }

        // POST /Admin/Reviews/Approve
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            review.IsApproved = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã duyệt đánh giá" });
        }

        // POST /Admin/Reviews/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa đánh giá" });
        }
    }
}
