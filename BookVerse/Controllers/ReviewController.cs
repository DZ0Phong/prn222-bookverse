using System.Security.Claims;
using BookVerse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers
{
    public class ReviewController : Controller
    {
        private readonly QuanLyBanSachContext _context;

        public ReviewController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // POST: /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int BookId, int Rating, string Comment)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account");
            }

            if (BookId <= 0 || Rating is < 1 or > 5 || string.IsNullOrWhiteSpace(Comment) || Comment.Length > 2000)
            {
                return RedirectToAction("Detail", "Book", new { id = BookId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Challenge();
            if (!await _context.Books.AnyAsync(b => b.BookId == BookId && b.IsActive != false)) return NotFound();

            var newReview = new Review
            {
                BookId = BookId,
                UserId = userId,
                Rating = Rating,
                Comment = Comment.Trim(),
                IsApproved = false, // MẶC ĐỊNH LÀ FALSE: Chờ Admin duyệt
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            // Gửi thông báo thành công qua TempData
            TempData["SuccessMessage"] = "Cảm ơn bạn! Đánh giá đã được gửi và đang chờ Admin kiểm duyệt trước khi hiển thị.";

            return RedirectToAction("Detail", "Book", new { id = BookId });
        }
    }
}
