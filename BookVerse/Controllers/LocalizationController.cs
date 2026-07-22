using BookVerse.Localization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.Controllers;

public sealed class LocalizationController : Controller
{
    private readonly IJsonLocalizer _localizer;
    public LocalizationController(IJsonLocalizer localizer) => _localizer = localizer;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        culture = culture == "en" ? "en" : "vi";
        Response.Cookies.Append("bookverse.culture", culture, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, HttpOnly = false, SameSite = SameSiteMode.Lax
        });
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : Redirect("/");
    }

    [HttpGet]
    public IActionResult ClientDictionary(string culture = "vi") => Json(_localizer.GetClientTextMap(culture));
}
