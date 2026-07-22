using BookVerse.Commerce;
using BookVerse.Configuration;
using Microsoft.Extensions.Options;

namespace BookVerse.Tests;

public sealed class CommercePricingServiceTests
{
    private static CommercePricingService Create() => new(Options.Create(new SiteSettings
    {
        ShippingFee = 30000,
        FreeShippingThreshold = 500000
    }));

    [Fact]
    public void Calculate_AppliesDiscountAndShipping()
    {
        var result = Create().Calculate(400000, 10);
        Assert.Equal(400000, result.Subtotal);
        Assert.Equal(40000, result.DiscountAmount);
        Assert.Equal(30000, result.ShippingFee);
        Assert.Equal(390000, result.GrandTotal);
    }

    [Fact]
    public void Calculate_UsesFreeShippingThresholdAfterDiscount()
    {
        var result = Create().Calculate(600000, 10);
        Assert.Equal(0, result.ShippingFee);
        Assert.Equal(540000, result.GrandTotal);
    }

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(150, 100)]
    public void Calculate_ClampsDiscount(int input, int expected)
    {
        Assert.Equal(expected, Create().Calculate(100000, input).DiscountPercent);
    }
}
