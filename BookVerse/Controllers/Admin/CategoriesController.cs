using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Controllers.Admin
{
    public class CategoriesController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public CategoriesController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Categories
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý danh mục";
            ViewData["ActiveMenu"] = "Categories";

            var categories = await _context.Categories
                .Select(c => new CategoryListItem
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    BookCount = c.Books.Count
                })
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            var vm = new CategoryListViewModel
            {
                Categories = categories,
                Form = new CategoryFormViewModel()
            };

            return View(vm);
        }

        // POST /Admin/Categories/Create (AJAX)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            // Check duplicate name
            if (await _context.Categories.AnyAsync(c => c.CategoryName == form.CategoryName))
                return Json(new { success = false, message = "Tên danh mục đã tồn tại!" });

            var category = new Category
            {
                CategoryName = form.CategoryName,
                Description = form.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm danh mục thành công!",
                data = new { category.CategoryId, category.CategoryName, category.Description, bookCount = 0 }
            });
        }

        // POST /Admin/Categories/Edit (AJAX)
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] CategoryFormViewModel form)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var category = await _context.Categories.FindAsync(form.CategoryId);
            if (category == null)
                return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            // Check duplicate name (exclude self)
            if (await _context.Categories.AnyAsync(c => c.CategoryName == form.CategoryName && c.CategoryId != form.CategoryId))
                return Json(new { success = false, message = "Tên danh mục đã tồn tại!" });

            category.CategoryName = form.CategoryName;
            category.Description = form.Description;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Cập nhật danh mục thành công!",
                data = new { category.CategoryId, category.CategoryName, category.Description }
            });
        }

        // POST /Admin/Categories/Delete (AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            if (category.Books.Any())
                return Json(new { success = false, message = $"Không thể xóa danh mục \"{category.CategoryName}\" vì còn {category.Books.Count} sách thuộc danh mục này!" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã xóa danh mục \"{category.CategoryName}\"!" });
        }

        // GET /Admin/Categories/GetById/{id} (AJAX - for edit modal)
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                data = new { category.CategoryId, category.CategoryName, category.Description }
            });
        }
    }
}
