using BookVerse.Models;

namespace BookVerse.Payments;

public interface IVnPayService
{
    bool IsConfigured { get; }
    string CreatePaymentUrl(Order order, HttpContext httpContext, string locale);
    VnPayResult ValidateResponse(IQueryCollection query);
}

public sealed record VnPayResult(
    bool IsValidSignature,
    bool IsSuccess,
    int? OrderId,
    decimal Amount,
    string ResponseCode,
    string TransactionStatus,
    string? TransactionNumber);
