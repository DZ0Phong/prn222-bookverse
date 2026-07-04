using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels.Admin
{
    public class VoucherListViewModel
    {
        public List<VoucherListItem> Vouchers { get; set; } = new();
        public VoucherFormViewModel Form { get; set; } = new();
    }

    public class VoucherListItem
    {
        public int VoucherId { get; set; }
        public string? Code { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool? IsActive { get; set; }
        public int UsedCount { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    }

    public class VoucherFormViewModel
    {
        public int VoucherId { get; set; }

        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [MaxLength(50, ErrorMessage = "Mã tối đa 50 ký tự")]
        [RegularExpression(@"^[A-Z0-9_\-]+$", ErrorMessage = "Mã chỉ chứa chữ hoa, số, dấu _ hoặc -")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phần trăm giảm giá là bắt buộc")]
        [Range(1, 100, ErrorMessage = "Phần trăm giảm phải từ 1 đến 100")]
        public int DiscountPercent { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn là bắt buộc")]
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddDays(30);

        public bool IsActive { get; set; } = true;
    }
}
