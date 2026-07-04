using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.Controllers.Admin
{
    /// <summary>
    /// Legacy route: /Admin/Auth/* → redirect to unified /Account/* endpoints.
    /// Authentication is now fully handled by AccountController.
    /// </summary>
    [Route("Admin/Auth/[action]")]
    public class AdminAuthController : Controller
    {
        // GET /Admin/Auth/Login → forward to /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
                return Redirect("/Admin/Dashboard");

            return Redirect("/Account/Login" + (returnUrl != null ? $"?returnUrl={Uri.EscapeDataString(returnUrl)}" : ""));
        }

        // GET /Admin/Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Account/Login");
        }

        // GET /Admin/Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return Redirect("/Account/AccessDenied");
        }
    }
}
