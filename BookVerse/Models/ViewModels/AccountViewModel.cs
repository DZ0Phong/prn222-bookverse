using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(150, ErrorMessage = "Họ tên tối đa 150 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        [RegularExpression(@"^[0-9+ ]{8,20}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string? Address { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Xác nhận mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
