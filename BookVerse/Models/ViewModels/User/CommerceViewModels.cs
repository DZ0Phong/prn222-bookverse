using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels.User;

public sealed class CartItemViewModel
{
    public int CartId { get; set; }
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal Subtotal => Price * Quantity;
}

public sealed class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(x => x.Subtotal);
    public int TotalQuantity => Items.Sum(x => x.Quantity);
}

public sealed class CheckoutViewModel
{
    public CartViewModel Cart { get; set; } = new();

    [Required, StringLength(255)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[0-9+ ]{8,20}$")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string PaymentMethod { get; set; } = "COD";

    [StringLength(50)]
    public string? VoucherCode { get; set; }

    public int? VoucherId { get; set; }
    public int DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal GrandTotal => Math.Max(0, Cart.Subtotal - DiscountAmount + ShippingFee);
    public bool EnableCod { get; set; }
    public bool EnableVnPay { get; set; }
    public bool VnPayConfigured { get; set; }
}

public sealed class PaymentResultViewModel
{
    public int? OrderId { get; set; }
    public bool Success { get; set; }
    public string MessageKey { get; set; } = string.Empty;
    public string? ResponseCode { get; set; }
}
