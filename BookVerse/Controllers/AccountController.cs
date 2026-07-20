using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Models.ViewModels.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookVerse.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyBanSachContext _context;

        public AccountController(QuanLyBanSachContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────
        // GET /Account/Login
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Đã đăng nhập thì không cho vào trang Login
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return Redirect("/Admin/Dashboard");
                return Redirect("/");
            }

            var vm = new LoginViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        // ─────────────────────────────────────────
        // POST /Account/Login
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Tìm user theo email, include Role
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

            // Không tiết lộ email có tồn tại hay không
            const string genericError = "Email hoặc mật khẩu không đúng.";

            if (user == null)
            {
                vm.ErrorMessage = genericError;
                return View(vm);
            }

            // Kiểm tra tài khoản bị khóa
            if (user.IsActive == false)
            {
                vm.ErrorMessage = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.";
                return View(vm);
            }

            // Kiểm tra mật khẩu (BCrypt)
            bool passwordValid = false;
            try
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(vm.Password, user.Password);
            }
            catch
            {
                // Nếu password trong DB là plain text (chưa hash) thì fallback so sánh trực tiếp
                passwordValid = (vm.Password == user.Password);
            }

            if (!passwordValid)
            {
                vm.ErrorMessage = genericError;
                return View(vm);
            }

            // Tạo Claims
            var roleName = user.Role?.RoleName ?? "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? "User"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("FullName", user.FullName ?? ""),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // Redirect sau đăng nhập
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            if (roleName == "Admin")
                return Redirect("/Admin/Dashboard");

            return Redirect("/");
        }

        // ─────────────────────────────────────────
        // GET /Account/Register
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Register(string? email = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/");

            return View(new RegisterViewModel { Email = email ?? string.Empty });
        }

        // ─────────────────────────────────────────
        // POST /Account/Register
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Email trùng?
            if (await _context.Users.AnyAsync(u => u.Email == vm.Email))
            {
                vm.ErrorMessage = "Email này đã được đăng ký. Vui lòng dùng email khác.";
                return View(vm);
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            var newUser = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                Password = hashedPassword,
                RoleId = 2, // 2 = User
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Tự động đăng nhập sau khi đăng ký
            var roleName = "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, newUser.FullName ?? newUser.Email ?? "User"),
                new Claim(ClaimTypes.Email, newUser.Email ?? ""),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("UserId", newUser.UserId.ToString()),
                new Claim("FullName", newUser.FullName ?? ""),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            return Redirect("/");
        }

        // ─────────────────────────────────────────
        // POST /Account/Logout
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

        // ─────────────────────────────────────────
        // GET /Account/AccessDenied
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }



        // ─────────────────────────────────────────
        // GET /Account/OrderHistory
        // ─────────────────────────────────────────
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            // Lấy UserId của tài khoản đang đăng nhập từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login");
            }

            // Lấy dữ liệu và map trực tiếp sang UserOrderListViewModel
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new UserOrderListViewModel
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    TotalItems = o.OrderDetails.Sum(d => d.Quantity ?? 0),
                    TotalAmount = o.TotalAmount ?? 0,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus
                })
                .ToListAsync();

            return View(orders);
        }
    }
}
