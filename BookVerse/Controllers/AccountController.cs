using BookVerse.Models;
using BookVerse.Models.ViewModels;
using BookVerse.Models.ViewModels.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookVerse.Commerce;
using BookVerse.Configuration;
using Microsoft.Extensions.Options;
using BookVerse.Localization;

namespace BookVerse.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyBanSachContext _context;
        private readonly SiteSettings _siteSettings;
        private readonly IJsonLocalizer _localizer;

        public AccountController(QuanLyBanSachContext context, IOptions<SiteSettings> siteSettings, IJsonLocalizer localizer)
        {
            _context = context;
            _siteSettings = siteSettings.Value;
            _localizer = localizer;
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
            var genericError = _localizer["Auth.InvalidCredentials"];

            if (user == null)
            {
                vm.ErrorMessage = genericError;
                return View(vm);
            }

            // Kiểm tra tài khoản bị khóa
            if (user.IsActive == false)
            {
                vm.ErrorMessage = _localizer["Auth.AccountLocked"];
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

            await MergeGuestCartAsync(user.UserId);

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
        public IActionResult Register(string? email = null, string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return Redirect("/");

            return View(new RegisterViewModel { Email = email ?? string.Empty, ReturnUrl = returnUrl });
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
                vm.ErrorMessage = _localizer["Auth.DuplicateEmail"];
                return View(vm);
            }

            // Hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password);

            var userRoleId = await _context.Roles
                .Where(r => r.RoleName == "User")
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();
            if (userRoleId == 0)
            {
                ModelState.AddModelError(string.Empty, _localizer["Auth.UserRoleMissing"]);
                return View(vm);
            }

            var newUser = new User
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                Password = hashedPassword,
                RoleId = userRoleId,
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

            await MergeGuestCartAsync(newUser.UserId);

            return !string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl)
                ? LocalRedirect(vm.ReturnUrl) : Redirect("/");
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

        [HttpGet("/Account/Profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await FindCurrentUserAsync();
            if (user == null) return Challenge();
            return View(new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty, Email = user.Email ?? string.Empty,
                Phone = user.Phone, Address = user.Address, CreatedAt = user.CreatedAt
            });
        }

        [HttpPost("/Account/Profile")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            var user = await FindCurrentUserAsync();
            if (user == null) return Challenge();
            vm.Email = user.Email ?? string.Empty;
            vm.CreatedAt = user.CreatedAt;
            if (!ModelState.IsValid) return View(vm);

            user.FullName = vm.FullName.Trim();
            user.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();
            user.Address = string.IsNullOrWhiteSpace(vm.Address) ? null : vm.Address.Trim();
            await _context.SaveChangesAsync();
            await RefreshSignInAsync(user);
            TempData["ProfileSuccess"] = "Thông tin tài khoản đã được cập nhật.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("/Account/ChangePassword")]
        [Authorize]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost("/Account/ChangePassword")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var user = await FindCurrentUserAsync();
            if (user == null) return Challenge();
            if (!PasswordMatches(vm.CurrentPassword, user.Password))
            {
                ModelState.AddModelError(nameof(vm.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                return View(vm);
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(vm.NewPassword);
            await _context.SaveChangesAsync();
            TempData["ProfileSuccess"] = "Mật khẩu đã được thay đổi thành công.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet("/Account/Orders")]
        [Authorize]
        public IActionResult Orders() => RedirectToAction(nameof(OrderHistory));



        // ─────────────────────────────────────────
        // GET /Account/OrderHistory
        // ─────────────────────────────────────────
        [HttpGet("/Account/OrderHistory")]
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

        [HttpGet("/Account/OrderDetail/{id:int}")]
        [Authorize]
        public async Task<IActionResult> OrderDetail(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId)) return Challenge();

            var order = await _context.Orders
                .Where(o => o.OrderId == id && o.UserId == userId)
                .Select(o => new UserOrderDetailViewModel
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    ShippingAddress = o.ShippingAddress,
                    PhoneNumber = o.PhoneNumber,
                    TotalAmount = o.TotalAmount ?? 0,
                    Items = o.OrderDetails.Select(d => new UserOrderItemViewModel
                    {
                        BookId = d.BookId ?? 0,
                        BookTitle = d.Book != null ? d.Book.Title : string.Empty,
                        BookImage = d.Book != null ? d.Book.Image : null,
                        Quantity = d.Quantity ?? 0,
                        Price = d.Price ?? 0
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return order == null ? NotFound() : View(order);
        }

        private async Task<User?> FindCurrentUserAsync()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == userId) : null;
        }

        private static bool PasswordMatches(string input, string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;
            try { return BCrypt.Net.BCrypt.Verify(input, stored); }
            catch { return input == stored; }
        }

        private async Task RefreshSignInAsync(User user)
        {
            var roleName = user.Role?.RoleName ?? "User";
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.FullName ?? user.Email ?? "User"),
                new(ClaimTypes.Email, user.Email ?? string.Empty), new(ClaimTypes.Role, roleName),
                new("UserId", user.UserId.ToString()), new("FullName", user.FullName ?? string.Empty)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        }

        private async Task MergeGuestCartAsync(int userId)
        {
            var guestCart = GuestCartStore.Read(HttpContext.Session);
            if (guestCart.Count == 0) return;
            var bookIds = guestCart.Keys.ToList();
            var stocks = await _context.Books.Where(x => bookIds.Contains(x.BookId) && x.IsActive != false)
                .ToDictionaryAsync(x => x.BookId, x => x.Quantity ?? 0);
            var existing = await _context.Carts.Where(x => x.UserId == userId && x.BookId.HasValue && bookIds.Contains(x.BookId.Value)).ToListAsync();
            foreach (var pair in guestCart)
            {
                if (!stocks.TryGetValue(pair.Key, out var stock) || stock <= 0) continue;
                var row = existing.FirstOrDefault(x => x.BookId == pair.Key);
                var quantity = Math.Min((row?.Quantity ?? 0) + pair.Value, Math.Min(stock, _siteSettings.MaxQuantityPerCartItem));
                if (row == null) _context.Carts.Add(new Cart { UserId = userId, BookId = pair.Key, Quantity = quantity });
                else row.Quantity = quantity;
            }
            await _context.SaveChangesAsync();
            GuestCartStore.Clear(HttpContext.Session);
        }
    }
}
