namespace BookVerse.Configuration;

public sealed class VnPayOptions
{
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string OrderType { get; set; } = "other";
    public bool AllowReturnToConfirmPayment { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(TmnCode)
                                && !string.IsNullOrWhiteSpace(HashSecret);
}
