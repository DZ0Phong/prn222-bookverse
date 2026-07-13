using System.Diagnostics;
using BookVerse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLyBanSachContext _context;

        public HomeController(ILogger<HomeController> logger, QuanLyBanSachContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> HomePage(string? search, int? categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategoryId = categoryId;

            // Handle Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim();
                var searchResults = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.IsActive != false && (b.Title!.Contains(cleanSearch) || b.Author!.Contains(cleanSearch)))
                    .ToListAsync();
                ViewBag.SearchResults = searchResults;
                return View();
            }

            // Handle Category Filter
            if (categoryId.HasValue)
            {
                var categoryResults = await _context.Books
                    .Include(b => b.Category)
                    .Where(b => b.IsActive != false && b.CategoryId == categoryId)
                    .ToListAsync();
                ViewBag.CategoryResults = categoryResults;
                ViewBag.CategoryName = categories.FirstOrDefault(c => c.CategoryId == categoryId)?.CategoryName ?? "Danh mục";
                return View();
            }

            // Standard landing page: load sections
            // 1. Featured / Best Seller (limit to 8)
            var bestSellers = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive != false)
                .OrderByDescending(b => b.OrderDetails.Count)
                .ThenBy(b => b.BookId)
                .Take(8)
                .ToListAsync();

            // 2. New Arrivals (limit to 8, sorted by CreatedAt DESC)
            var newArrivals = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive != false)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .ToListAsync();

            // 3. Promotions/Discounted books (limit to 8)
            var promotions = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.IsActive != false)
                .OrderBy(b => b.BookId)
                .Skip(2) // Mix it up
                .Take(8)
                .ToListAsync();

            ViewBag.BestSellers = bestSellers;
            ViewBag.NewArrivals = newArrivals;
            ViewBag.Promotions = promotions;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
