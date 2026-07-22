using System.Security.Claims;
using BookVerse.Configuration;
using BookVerse.Domain;
using BookVerse.Models;
using BookVerse.Models.ViewModels.User;
using BookVerse.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BookVerse.Commerce;

namespace BookVerse.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public sealed class CheckoutController : Controller
{
    private readonly QuanLyBanSachContext _context;
    private readonly SiteSettings _settings;
    private readonly IVnPayService _vnPay;
    private readonly ICommercePricingService _pricing;

    public CheckoutController(QuanLyBanSachContext context, IOptions<SiteSettings> settings, IVnPayService vnPay, ICommercePricingService pricing)
    {
        _context = context;
        _settings = settings.Value;
        _vnPay = vnPay;
        _pricing = pricing;
    }

    [HttpGet("/Checkout")]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var user = await _context.Users.FindAsync(userId);
        var vm = await BuildViewModelAsync(userId, null);
        vm.ShippingAddress = user?.Address ?? string.Empty;
        vm.PhoneNumber = user?.Phone ?? string.Empty;
        if (!vm.Cart.Items.Any()) return RedirectToAction("Index", "Cart");
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyVoucher(string code)
    {
        var cart = await LoadCartAsync(GetUserId());
        var voucher = await FindVoucherAsync(code);
        if (voucher == null) return Json(new { success = false, messageKey = "Checkout.InvalidVoucher" });
        var price = _pricing.Calculate(cart.Subtotal, voucher.DiscountPercent ?? 0);
        return Json(new
        {
            success = true, voucherId = voucher.VoucherId, discountPercent = voucher.DiscountPercent ?? 0,
            discountAmount = price.DiscountAmount, shippingFee = price.ShippingFee,
            grandTotal = price.GrandTotal
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel input)
    {
        var userId = GetUserId();
        var vm = await BuildViewModelAsync(userId, input.VoucherCode);
        vm.ShippingAddress = input.ShippingAddress;
        vm.PhoneNumber = input.PhoneNumber;
        vm.PaymentMethod = input.PaymentMethod;
        vm.VoucherCode = input.VoucherCode;

        if (!vm.Cart.Items.Any()) ModelState.AddModelError(string.Empty, "Checkout.EmptyCart");
        if (input.PaymentMethod == OrderValues.PaymentMethod.Cod && !_settings.EnableCod)
            ModelState.AddModelError(nameof(input.PaymentMethod), "Checkout.PaymentUnavailable");
        if (input.PaymentMethod == OrderValues.PaymentMethod.VnPay && (!_settings.EnableVnPay || !_vnPay.IsConfigured))
            ModelState.AddModelError(nameof(input.PaymentMethod), "Checkout.VnPayNotConfigured");
        if (input.PaymentMethod is not (OrderValues.PaymentMethod.Cod or OrderValues.PaymentMethod.VnPay))
            ModelState.AddModelError(nameof(input.PaymentMethod), "Checkout.PaymentUnavailable");
        if (!ModelState.IsValid) return View("Index", vm);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var cartRows = await _context.Carts.Include(x => x.Book)
                .Where(x => x.UserId == userId).ToListAsync();
            foreach (var row in cartRows)
            {
                if (row.Book == null || row.Book.IsActive == false || (row.Quantity ?? 0) <= 0
                    || (row.Book.Quantity ?? 0) < (row.Quantity ?? 0))
                {
                    ModelState.AddModelError(string.Empty, "Checkout.StockChanged");
                    await transaction.RollbackAsync();
                    return View("Index", await BuildViewModelAsync(userId, input.VoucherCode));
                }
            }

            var order = new Order
            {
                UserId = userId, VoucherId = vm.VoucherId, TotalAmount = vm.GrandTotal,
                ShippingAddress = input.ShippingAddress.Trim(), PhoneNumber = input.PhoneNumber.Trim(),
                Status = OrderValues.Status.Pending, PaymentMethod = input.PaymentMethod,
                PaymentStatus = input.PaymentMethod == OrderValues.PaymentMethod.VnPay
                    ? OrderValues.PaymentStatus.PendingVnPay : OrderValues.PaymentStatus.Unpaid,
                CreatedAt = DateTime.Now
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var row in cartRows)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.OrderId, BookId = row.BookId, Quantity = row.Quantity,
                    Price = row.Book!.Price ?? 0
                });
                row.Book.Quantity = (row.Book.Quantity ?? 0) - (row.Quantity ?? 0);
            }
            _context.Carts.RemoveRange(cartRows);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (input.PaymentMethod == OrderValues.PaymentMethod.VnPay)
            {
                var locale = Request.Cookies["bookverse.culture"] ?? "vi";
                return Redirect(_vnPay.CreatePaymentUrl(order, HttpContext, locale));
            }
            TempData["SuccessKey"] = "Checkout.OrderPlaced";
            return Redirect($"/Account/OrderDetail/{order.OrderId}");
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Checkout.UnexpectedError");
            return View("Index", vm);
        }
    }

    private async Task<CheckoutViewModel> BuildViewModelAsync(int userId, string? voucherCode)
    {
        var cart = await LoadCartAsync(userId);
        var voucher = await FindVoucherAsync(voucherCode);
        var price = _pricing.Calculate(cart.Subtotal, voucher?.DiscountPercent ?? 0);
        return new CheckoutViewModel
        {
            Cart = cart, VoucherCode = voucher?.Code, VoucherId = voucher?.VoucherId,
            DiscountPercent = price.DiscountPercent, DiscountAmount = price.DiscountAmount,
            ShippingFee = price.ShippingFee, EnableCod = _settings.EnableCod,
            EnableVnPay = _settings.EnableVnPay, VnPayConfigured = _vnPay.IsConfigured
        };
    }

    private async Task<CartViewModel> LoadCartAsync(int userId) => new()
    {
        Items = await _context.Carts.Where(x => x.UserId == userId && x.Book != null)
            .Select(x => new CartItemViewModel
            {
                CartId = x.CartId, BookId = x.BookId ?? 0, Title = x.Book!.Title ?? string.Empty,
                Image = x.Book.Image, Price = x.Book.Price ?? 0, Quantity = x.Quantity ?? 0,
                Stock = x.Book.Quantity ?? 0
            }).ToListAsync()
    };

    private Task<Voucher?> FindVoucherAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return Task.FromResult<Voucher?>(null);
        var normalized = code.Trim().ToUpperInvariant();
        return _context.Vouchers.FirstOrDefaultAsync(x => x.Code == normalized && x.IsActive == true
            && (!x.ExpiryDate.HasValue || x.ExpiryDate.Value >= DateTime.Now));
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException());
}
