using BookVerse.Models;
using BookVerse.Models.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers
{
    [Route("User/[controller]/[action]/{id?}")]
    public class BookController : Controller
    {
        private readonly QuanLyBanSachContext _context;

        public BookController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Book/Detail/{id}
        public async Task<IActionResult> Detail(int id)
        {
            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null) return NotFound();

            // CHỈ LẤY CÁC ĐÁNH GIÁ ĐÃ ĐƯỢC ADMIN DUYỆT (IsApproved == true)
            var approvedReviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == id && r.IsApproved == true)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var viewModel = new BookDetailViewModel
            {
                Book = book,
                ExistingReviews = approvedReviews
            };

            return View("~/Views/User/Book/Detail.cshtml", viewModel);
        }
    }
}
