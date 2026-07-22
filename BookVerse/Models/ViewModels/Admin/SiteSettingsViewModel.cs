using System.ComponentModel.DataAnnotations;
namespace BookVerse.Models.ViewModels.Admin;
public sealed class SiteSettingsViewModel
{
    [Required, StringLength(100)] public string BrandName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string SupportEmail { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Hotline { get; set; } = string.Empty;
    [Required, StringLength(255)] public string Address { get; set; } = string.Empty;
    [Required, StringLength(3, MinimumLength = 3)] public string CurrencyCode { get; set; } = "VND";
    [Range(0, 10000000)] public decimal ShippingFee { get; set; }
    [Range(0, 100000000)] public decimal FreeShippingThreshold { get; set; }
    [Range(1, 1000)] public int MaxQuantityPerCartItem { get; set; }
    [Range(5, 1440)] public int PaymentTimeoutMinutes { get; set; }
    public bool EnableCod { get; set; }
    public bool EnableVnPay { get; set; }
}
