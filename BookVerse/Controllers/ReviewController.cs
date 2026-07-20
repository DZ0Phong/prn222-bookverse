using System.Security.Claims;
using BookVerse.Models;
using Microsoft.AspNetCore.Mvc;

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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (BookId <= 0 || string.IsNullOrWhiteSpace(Comment))
            {
                return RedirectToAction("Detail", "Book", new { id = BookId });
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int userId);

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
