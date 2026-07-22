using System.Security.Claims;
using BookVerse.Commerce;
using BookVerse.Configuration;
using BookVerse.Models;
using BookVerse.Models.ViewModels.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookVerse.Controllers;

[Route("[controller]/[action]")]
public sealed class CartController : Controller
{
    private readonly QuanLyBanSachContext _context;
    private readonly SiteSettings _settings;
    public CartController(QuanLyBanSachContext context, IOptions<SiteSettings> settings) { _context = context; _settings = settings.Value; }

    [HttpGet("/Cart")]
    public async Task<IActionResult> Index() => View(TryGetUserId(out var userId)
        ? await LoadUserCartAsync(userId) : await LoadGuestCartAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int bookId, int quantity = 1, string? returnUrl = null)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.BookId == bookId && x.IsActive != false);
        if (book == null) return NotFound();
        if ((book.Quantity ?? 0) <= 0) return BadRequest(new { success = false, message = "Cart.OutOfStock" });
        quantity = Math.Clamp(quantity, 1, _settings.MaxQuantityPerCartItem);
        int count;
        if (TryGetUserId(out var userId))
        {
            var item = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == bookId);
            var requested = Math.Min((item?.Quantity ?? 0) + quantity, Math.Min(book.Quantity ?? 0, _settings.MaxQuantityPerCartItem));
            if (item == null) _context.Carts.Add(new Cart { UserId = userId, BookId = bookId, Quantity = requested });
            else item.Quantity = requested;
            await _context.SaveChangesAsync();
            count = await UserCartCountAsync(userId);
        }
        else
        {
            var cart = GuestCartStore.Read(HttpContext.Session);
            cart[bookId] = Math.Min(cart.GetValueOrDefault(bookId) + quantity, Math.Min(book.Quantity ?? 0, _settings.MaxQuantityPerCartItem));
            GuestCartStore.Write(HttpContext.Session, cart);
            count = cart.Values.Sum();
        }
        if (Request.Headers.Accept.Any(x => x?.Contains("application/json") == true)) return Json(new { success = true, count, message = "Cart.Added" });
        TempData["SuccessKey"] = "Cart.Added";
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int cartId, int quantity)
    {
        if (TryGetUserId(out var userId))
        {
            var item = await _context.Carts.Include(x => x.Book).FirstOrDefaultAsync(x => x.CartId == cartId && x.UserId == userId);
            if (item == null) return NotFound();
            if (quantity <= 0) _context.Carts.Remove(item);
            else item.Quantity = Math.Min(quantity, Math.Min(item.Book?.Quantity ?? 0, _settings.MaxQuantityPerCartItem));
            await _context.SaveChangesAsync();
        }
        else
        {
            var cart = GuestCartStore.Read(HttpContext.Session);
            var book = await _context.Books.FindAsync(cartId);
            if (!cart.ContainsKey(cartId) || book == null) return NotFound();
            if (quantity <= 0) cart.Remove(cartId);
            else cart[cartId] = Math.Min(quantity, Math.Min(book.Quantity ?? 0, _settings.MaxQuantityPerCartItem));
            GuestCartStore.Write(HttpContext.Session, cart);
        }
        TempData["SuccessKey"] = "Cart.Updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartId)
    {
        if (TryGetUserId(out var userId))
        {
            var item = await _context.Carts.FirstOrDefaultAsync(x => x.CartId == cartId && x.UserId == userId);
            if (item != null) { _context.Carts.Remove(item); await _context.SaveChangesAsync(); }
        }
        else
        {
            var cart = GuestCartStore.Read(HttpContext.Session); cart.Remove(cartId); GuestCartStore.Write(HttpContext.Session, cart);
        }
        TempData["SuccessKey"] = "Cart.Removed";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Count() => Json(new { count = TryGetUserId(out var userId) ? await UserCartCountAsync(userId) : GuestCartStore.Read(HttpContext.Session).Values.Sum() });

    private async Task<CartViewModel> LoadUserCartAsync(int userId) => new() { Items = await _context.Carts.Where(x => x.UserId == userId && x.Book != null).OrderByDescending(x => x.CartId).Select(x => new CartItemViewModel { CartId = x.CartId, BookId = x.BookId ?? 0, Title = x.Book!.Title ?? string.Empty, Image = x.Book.Image, Price = x.Book.Price ?? 0, Quantity = x.Quantity ?? 0, Stock = x.Book.Quantity ?? 0 }).ToListAsync() };
    private async Task<CartViewModel> LoadGuestCartAsync()
    {
        var cart = GuestCartStore.Read(HttpContext.Session); var ids = cart.Keys.ToList();
        var books = await _context.Books.Where(x => ids.Contains(x.BookId) && x.IsActive != false).ToListAsync();
        return new CartViewModel { Items = books.Select(x => new CartItemViewModel { CartId = x.BookId, BookId = x.BookId, Title = x.Title ?? string.Empty, Image = x.Image, Price = x.Price ?? 0, Quantity = cart.GetValueOrDefault(x.BookId), Stock = x.Quantity ?? 0 }).ToList() };
    }
    private Task<int> UserCartCountAsync(int userId) => _context.Carts.Where(x => x.UserId == userId).SumAsync(x => x.Quantity ?? 0);
    private bool TryGetUserId(out int userId) => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
