namespace BookVerse.Commerce;

public interface ICommercePricingService
{
    PricingResult Calculate(decimal subtotal, int discountPercent);
}

public sealed record PricingResult(decimal Subtotal, int DiscountPercent, decimal DiscountAmount,
    decimal ShippingFee, decimal GrandTotal);
