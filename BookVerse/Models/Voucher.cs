using System;
using System.Collections.Generic;

namespace BookVerse.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string? Code { get; set; }

    public int? DiscountPercent { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
