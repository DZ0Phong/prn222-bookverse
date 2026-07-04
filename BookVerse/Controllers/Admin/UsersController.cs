using BookVerse.Models;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookVerse.Controllers.Admin
{
    public class UsersController : AdminBaseController
    {
        private readonly QuanLyBanSachContext _context;

        public UsersController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // GET /Admin/Users
        public async Task<IActionResult> Index(string? search, int? roleId, bool? isActive, int page = 1)
        {
            ViewData["Title"] = "Quản lý người dùng";
            ViewData["ActiveMenu"] = "Users";

            const int pageSize = 15;

            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.FullName!.Contains(search) || u.Email!.Contains(search));

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItem
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleName = u.Role != null ? u.Role.RoleName : "N/A",
                    RoleId = u.RoleId,
                    CreatedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            var vm = new UserListViewModel
            {
                Users = users,
                Roles = await _context.Roles.ToListAsync(),
                SearchTerm = search,
                FilterRoleId = roleId,
                FilterIsActive = isActive,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(vm);
        }

        // POST /Admin/Users/ToggleLock
        [HttpPost]
        public async Task<IActionResult> ToggleLock(int id)
        {
            // Get current admin's ID
            var currentUserIdStr = User.FindFirstValue("UserId");
            if (!int.TryParse(currentUserIdStr, out int currentUserId))
                return Json(new { success = false, message = "Không thể xác định danh tính admin" });

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng" });

            // Can't lock yourself
            if (user.UserId == currentUserId)
                return Json(new { success = false, message = "Bạn không thể tự khóa tài khoản của mình!" });

            // Can't lock another admin
            if (user.Role?.RoleName == "Admin")
                return Json(new { success = false, message = "Không thể khóa tài khoản Admin khác!" });

            user.IsActive = !(user.IsActive ?? true);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = user.IsActive,
                message = user.IsActive == true ? $"Đã mở khóa tài khoản {user.FullName}" : $"Đã khóa tài khoản {user.FullName}"
            });
        }
    }
}
