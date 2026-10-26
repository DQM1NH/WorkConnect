using System.ComponentModel.DataAnnotations;

namespace WorkConnect.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyOtpViewModel
    {
        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [RegularExpression(@"^\s*\d{6}\s*$", ErrorMessage = "Mã OTP gồm 6 chữ số")]
        public string OtpCode { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        // Cùng quy tắc với đăng ký: 8-100 ký tự
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên")]
        [MaxLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
