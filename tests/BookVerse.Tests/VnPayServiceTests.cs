using System.Net;
using System.Security.Cryptography;
using System.Text;
using BookVerse.Configuration;
using BookVerse.Models;
using BookVerse.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace BookVerse.Tests;

public sealed class VnPayServiceTests
{
    [Fact]
    public void CreatePaymentUrl_UsesV21AmountTimes100AndValidHmac()
    {
        const string secret = "test-secret";
        var service = new VnPayService(Options.Create(new VnPayOptions
        {
            TmnCode = "TESTCODE",
            HashSecret = secret,
            ReturnUrl = "https://merchant.test/Payment/VnPayReturn"
        }), Options.Create(new SiteSettings { PaymentTimeoutMinutes = 15 }));
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;

        var url = service.CreatePaymentUrl(new Order { OrderId = 42, TotalAmount = 516000 }, context, "vi");
        var uri = new Uri(url);
        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.Equal("2.1.0", query["vnp_Version"]);
        Assert.Equal("51600000", query["vnp_Amount"]);
        Assert.Equal("42", query["vnp_TxnRef"]);

        var unsigned = string.Join("&", query.Where(x => x.Key != "vnp_SecureHash")
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value.ToString())}"));
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned))).ToLowerInvariant();
        Assert.Equal(expected, query["vnp_SecureHash"].ToString());
    }

    [Fact]
    public void ValidateResponse_RejectsTamperedSignature()
    {
        var service = new VnPayService(Options.Create(new VnPayOptions
        {
            TmnCode = "TESTCODE", HashSecret = "test-secret"
        }), Options.Create(new SiteSettings()));
        var query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            ["vnp_TxnRef"] = "42", ["vnp_Amount"] = "10000",
            ["vnp_ResponseCode"] = "00", ["vnp_TransactionStatus"] = "00",
            ["vnp_SecureHash"] = "tampered"
        });
        Assert.False(service.ValidateResponse(query).IsValidSignature);
    }
}
