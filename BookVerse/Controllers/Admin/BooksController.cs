using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class BooksController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(QuanLyBanSachContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET /Admin/Books
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            ViewData["Title"] = "Quản lý sách";
            ViewData["ActiveMenu"] = "Books";

            const int pageSize = 10;

            var query = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(b => b.Title!.Contains(search) || b.Author!.Contains(search));

            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var books = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookListItem
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Author = b.Author,
                    CategoryName = b.Category != null ? b.Category.CategoryName : "Chưa phân loại",
                    Price = b.Price,
                    Quantity = b.Quantity,
                    Image = b.Image,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    HasOrders = b.OrderDetails.Any()
                })
                .ToListAsync();

            var categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            var vm = new BookListViewModel
            {
                Books = books,
                Categories = categories,
                SearchTerm = search,
                FilterCategoryId = categoryId,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(vm);
        }

        // GET /Admin/Books/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm sách mới";
            ViewData["ActiveMenu"] = "Books";

            var vm = new BookFormViewModel
            {
                Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(),
                IsActive = true
            };
            return View("CreateEdit", vm);
        }

        // POST /Admin/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel vm)
        {
            ViewData["Title"] = "Thêm sách mới";
            ViewData["ActiveMenu"] = "Books";

            vm.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            if (!ModelState.IsValid)
                return View("CreateEdit", vm);

            // Handle image upload
            string? imagePath = null;
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var result = await SaveImageAsync(vm.ImageFile);
                if (result.Error != null)
                {
                    ModelState.AddModelError("ImageFile", result.Error);
                    return View("CreateEdit", vm);
                }
                imagePath = result.Path;
            }

            var book = new Book
            {
                Title = vm.Title,
                Author = vm.Author,
                Publisher = vm.Publisher,
                CategoryId = vm.CategoryId,
                Price = vm.Price,
                Quantity = vm.Quantity,
                Description = vm.Description,
                Image = imagePath,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm sách \"{book.Title}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET /Admin/Books/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa sách";
            ViewData["ActiveMenu"] = "Books";

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy sách.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new BookFormViewModel
            {
                BookId = book.BookId,
                Title = book.Title ?? "",
                Author = book.Author ?? "",
                Publisher = book.Publisher,
                CategoryId = book.CategoryId ?? 0,
                Price = book.Price ?? 0,
                Quantity = book.Quantity ?? 0,
                Description = book.Description,
                ExistingImage = book.Image,
                IsActive = book.IsActive ?? true,
                Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync()
            };
            return View("CreateEdit", vm);
        }

        // POST /Admin/Books/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookFormViewModel vm)
        {
            ViewData["Title"] = "Chỉnh sửa sách";
            ViewData["ActiveMenu"] = "Books";

            vm.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            if (!ModelState.IsValid)
                return View("CreateEdit", vm);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy sách.";
                return RedirectToAction(nameof(Index));
            }

            // Handle new image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var result = await SaveImageAsync(vm.ImageFile);
                if (result.Error != null)
                {
                    ModelState.AddModelError("ImageFile", result.Error);
                    return View("CreateEdit", vm);
                }
                // Delete old image if exists
                if (!string.IsNullOrEmpty(book.Image))
                    DeleteImage(book.Image);

                book.Image = result.Path;
            }

            book.Title = vm.Title;
            book.Author = vm.Author;
            book.Publisher = vm.Publisher;
            book.CategoryId = vm.CategoryId;
            book.Price = vm.Price;
            book.Quantity = vm.Quantity;
            book.Description = vm.Description;
            book.IsActive = vm.IsActive;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật sách \"{book.Title}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST /Admin/Books/ToggleActive
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return Json(new { success = false, message = "Không tìm thấy sách" });

            book.IsActive = !(book.IsActive ?? true);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = book.IsActive, message = book.IsActive == true ? "Đã kích hoạt" : "Đã ẩn sách" });
        }

        // POST /Admin/Books/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books
                .Include(b => b.OrderDetails)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                TempData["Error"] = "Không tìm thấy sách.";
                return RedirectToAction(nameof(Index));
            }

            if (book.OrderDetails.Any())
            {
                // Soft delete — chỉ ẩn đi
                book.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Warning"] = $"Sách \"{book.Title}\" đã có trong đơn hàng nên không thể xóa cứng. Đã ẩn sách thay thế.";
            }
            else
            {
                // Hard delete if no orders
                if (!string.IsNullOrEmpty(book.Image))
                    DeleteImage(book.Image);

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa sách \"{book.Title}\" thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // ===== Helpers =====

        private async Task<(string? Path, string? Error)> SaveImageAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(ext))
                return (null, "Chỉ chấp nhận file ảnh: JPG, PNG, WEBP, GIF");

            if (file.Length > 5 * 1024 * 1024)
                return (null, "Kích thước ảnh tối đa 5MB");

            var uploadDir = Path.Combine(_env.WebRootPath, "images", "books");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return ($"/images/books/{fileName}", null);
        }

        private void DeleteImage(string imagePath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch { /* Ignore delete errors */ }
        }
    }
}
