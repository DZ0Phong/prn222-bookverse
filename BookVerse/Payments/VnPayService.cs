using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BookVerse.Configuration;
using BookVerse.Models;
using Microsoft.Extensions.Options;

namespace BookVerse.Payments;

public sealed class VnPayService : IVnPayService
{
    private readonly VnPayOptions _options;
    private readonly SiteSettings _siteSettings;

    public VnPayService(IOptions<VnPayOptions> options, IOptions<SiteSettings> siteSettings)
    {
        _options = options.Value;
        _siteSettings = siteSettings.Value;
    }

    public bool IsConfigured => _options.IsConfigured;

    public string CreatePaymentUrl(Order order, HttpContext httpContext, string locale)
    {
        if (!IsConfigured) throw new InvalidOperationException("VNPay merchant credentials are not configured.");
        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
        var returnUrl = string.IsNullOrWhiteSpace(_options.ReturnUrl)
            ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/Payment/VnPayReturn"
            : _options.ReturnUrl;
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _options.Version,
            ["vnp_Command"] = _options.Command,
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = decimal.ToInt64((order.TotalAmount ?? 0) * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CreateDate"] = now.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = _siteSettings.CurrencyCode,
            ["vnp_IpAddr"] = GetIpAddress(httpContext),
            ["vnp_Locale"] = string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don hang {order.OrderId}",
            ["vnp_OrderType"] = _options.OrderType,
            ["vnp_ReturnUrl"] = returnUrl,
            ["vnp_TxnRef"] = order.OrderId.ToString(CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = now.AddMinutes(_siteSettings.PaymentTimeoutMinutes).ToString("yyyyMMddHHmmss")
        };
        var query = BuildQuery(values);
        var signature = HmacSha512(_options.HashSecret, query);
        return $"{_options.PaymentUrl}?{query}&vnp_SecureHash={signature}";
    }

    public VnPayResult ValidateResponse(IQueryCollection query)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in query)
        {
            if (pair.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase)
                && pair.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
                values[pair.Key] = pair.Value.ToString();
        }
        var received = query["vnp_SecureHash"].ToString();
        var expected = HmacSha512(_options.HashSecret, BuildQuery(values));
        var valid = IsConfigured && FixedTimeEquals(received, expected);
        int.TryParse(query["vnp_TxnRef"], out var orderId);
        long.TryParse(query["vnp_Amount"], out var amountTimes100);
        var responseCode = query["vnp_ResponseCode"].ToString();
        var transactionStatus = query["vnp_TransactionStatus"].ToString();
        return new VnPayResult(valid, valid && responseCode == "00" && transactionStatus == "00",
            orderId > 0 ? orderId : null, amountTimes100 / 100m, responseCode, transactionStatus,
            query["vnp_TransactionNo"].ToString());
    }

    private static string BuildQuery(IEnumerable<KeyValuePair<string, string>> values) => string.Join("&",
        values.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        if (left.Length != right.Length) return false;
        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(left), Encoding.ASCII.GetBytes(right));
    }

    private static string GetIpAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
}
