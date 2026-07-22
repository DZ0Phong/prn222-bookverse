using BookVerse.Configuration;
using Microsoft.Extensions.Options;

namespace BookVerse.Commerce;

public sealed class CommercePricingService : ICommercePricingService
{
    private readonly SiteSettings _settings;
    public CommercePricingService(IOptions<SiteSettings> settings) => _settings = settings.Value;

    public PricingResult Calculate(decimal subtotal, int discountPercent)
    {
        subtotal = Math.Max(0, subtotal);
        discountPercent = Math.Clamp(discountPercent, 0, 100);
        var discount = Math.Round(subtotal * discountPercent / 100m, 0, MidpointRounding.AwayFromZero);
        var afterDiscount = Math.Max(0, subtotal - discount);
        var shipping = afterDiscount >= _settings.FreeShippingThreshold ? 0 : _settings.ShippingFee;
        return new PricingResult(subtotal, discountPercent, discount, shipping, afterDiscount + shipping);
    }
}
