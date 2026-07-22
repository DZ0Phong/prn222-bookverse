using BookVerse.Domain;
using BookVerse.Models;
using BookVerse.Models.ViewModels.User;
using BookVerse.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookVerse.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BookVerse.Controllers;

public sealed class PaymentController : Controller
{
    private readonly QuanLyBanSachContext _context;
    private readonly IVnPayService _vnPay;
    private readonly VnPayOptions _options;
    public PaymentController(QuanLyBanSachContext context, IVnPayService vnPay, IOptions<VnPayOptions> options)
    { _context = context; _vnPay = vnPay; _options = options.Value; }

    [HttpGet]
    public async Task<IActionResult> VnPayReturn()
    {
        var result = _vnPay.ValidateResponse(Request.Query);
        var order = result.OrderId.HasValue ? await _context.Orders.FindAsync(result.OrderId.Value) : null;
        if (!result.IsValidSignature || order == null || order.PaymentMethod != OrderValues.PaymentMethod.VnPay
            || order.TotalAmount != result.Amount)
            return View("Result", new PaymentResultViewModel { OrderId = result.OrderId, MessageKey = "Payment.InvalidSignature", ResponseCode = result.ResponseCode });

        if (_options.AllowReturnToConfirmPayment && result.IsSuccess && order.PaymentStatus != OrderValues.PaymentStatus.Paid)
        {
            order.PaymentStatus = OrderValues.PaymentStatus.Paid;
            await _context.SaveChangesAsync();
        }
        else if (!result.IsSuccess && order.PaymentStatus == OrderValues.PaymentStatus.PendingVnPay)
        {
            order.PaymentStatus = OrderValues.PaymentStatus.Failed;
            await _context.SaveChangesAsync();
        }
        return View("Result", new PaymentResultViewModel
        {
            OrderId = order.OrderId, Success = result.IsSuccess,
            MessageKey = result.IsSuccess ? "Payment.Success" : "Payment.Failed", ResponseCode = result.ResponseCode
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Retry(int orderId)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Challenge();
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId && x.UserId == userId);
        if (order == null) return NotFound();
        if (order.PaymentMethod != OrderValues.PaymentMethod.VnPay
            || order.PaymentStatus == OrderValues.PaymentStatus.Paid) return BadRequest();
        if (!_vnPay.IsConfigured) return View("Result", new PaymentResultViewModel
        {
            OrderId = order.OrderId, MessageKey = "Checkout.VnPayNotConfigured"
        });
        order.PaymentStatus = OrderValues.PaymentStatus.PendingVnPay;
        await _context.SaveChangesAsync();
        var culture = Request.Cookies["bookverse.culture"] ?? "vi";
        return Redirect(_vnPay.CreatePaymentUrl(order, HttpContext, culture));
    }

    [HttpGet]
    public async Task<IActionResult> VnPayIpn()
    {
        var result = _vnPay.ValidateResponse(Request.Query);
        if (!result.IsValidSignature) return Json(new { RspCode = "97", Message = "Invalid signature" });
        if (!result.OrderId.HasValue) return Json(new { RspCode = "01", Message = "Order not found" });
        var order = await _context.Orders.FindAsync(result.OrderId.Value);
        if (order == null) return Json(new { RspCode = "01", Message = "Order not found" });
        if (order.PaymentMethod != OrderValues.PaymentMethod.VnPay)
            return Json(new { RspCode = "01", Message = "Order not found" });
        if (order.TotalAmount != result.Amount) return Json(new { RspCode = "04", Message = "Invalid amount" });
        if (order.PaymentStatus == OrderValues.PaymentStatus.Paid) return Json(new { RspCode = "02", Message = "Order already confirmed" });
        order.PaymentStatus = result.IsSuccess ? OrderValues.PaymentStatus.Paid : OrderValues.PaymentStatus.Failed;
        await _context.SaveChangesAsync();
        return Json(new { RspCode = "00", Message = "Confirm success" });
    }
}
