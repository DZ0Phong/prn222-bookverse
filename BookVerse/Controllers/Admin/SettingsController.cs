using System.Text.Json;
using BookVerse.Configuration;
using BookVerse.Models.ViewModels.Admin;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
namespace BookVerse.Controllers.Admin;
public sealed class SettingsController : AdminBaseController
{
    private readonly IWebHostEnvironment _environment;
    private readonly IOptionsSnapshot<SiteSettings> _options;
    public SettingsController(IWebHostEnvironment environment, IOptionsSnapshot<SiteSettings> options) { _environment = environment; _options = options; }
    [HttpGet] public IActionResult Index() => View(ToViewModel(_options.Value));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettingsViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var settings = new SiteSettings { BrandName = model.BrandName.Trim(), SupportEmail = model.SupportEmail.Trim(), Hotline = model.Hotline.Trim(), Address = model.Address.Trim(), CurrencyCode = model.CurrencyCode.Trim().ToUpperInvariant(), ShippingFee = model.ShippingFee, FreeShippingThreshold = model.FreeShippingThreshold, MaxQuantityPerCartItem = model.MaxQuantityPerCartItem, PaymentTimeoutMinutes = model.PaymentTimeoutMinutes, EnableCod = model.EnableCod, EnableVnPay = model.EnableVnPay };
        var path = Path.Combine(_environment.ContentRootPath, "Config", "site-settings.json");
        var json = JsonSerializer.Serialize(new { SiteSettings = settings }, new JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(path, json);
        TempData["Success"] = "Đã lưu cấu hình. Các yêu cầu mới sẽ dùng giá trị vừa cập nhật.";
        return RedirectToAction(nameof(Index));
    }
    private static SiteSettingsViewModel ToViewModel(SiteSettings x) => new() { BrandName = x.BrandName, SupportEmail = x.SupportEmail, Hotline = x.Hotline, Address = x.Address, CurrencyCode = x.CurrencyCode, ShippingFee = x.ShippingFee, FreeShippingThreshold = x.FreeShippingThreshold, MaxQuantityPerCartItem = x.MaxQuantityPerCartItem, PaymentTimeoutMinutes = x.PaymentTimeoutMinutes, EnableCod = x.EnableCod, EnableVnPay = x.EnableVnPay };
}
