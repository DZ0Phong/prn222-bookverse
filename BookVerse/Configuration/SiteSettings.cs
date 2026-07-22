namespace BookVerse.Configuration;

public sealed class SiteSettings
{
    public string BrandName { get; set; } = "BookVerse";
    public string SupportEmail { get; set; } = "contact@bookverse.vn";
    public string Hotline { get; set; } = "1800 0000";
    public string Address { get; set; } = "Ha Noi, Viet Nam";
    public string CurrencyCode { get; set; } = "VND";
    public decimal ShippingFee { get; set; } = 30000;
    public decimal FreeShippingThreshold { get; set; } = 500000;
    public int MaxQuantityPerCartItem { get; set; } = 20;
    public int PaymentTimeoutMinutes { get; set; } = 15;
    public bool EnableCod { get; set; } = true;
    public bool EnableVnPay { get; set; } = true;
}
