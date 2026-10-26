
using System.ComponentModel.DataAnnotations;

namespace WorkConnect.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ và tên lót")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true",
            ErrorMessage = "Bạn cần đồng ý với điều khoản sử dụng")]
        public bool AgreeTerms { get; set; }

        // Vai trò đăng ký: Candidate hoặc Employer.
        [Required(ErrorMessage = "Vui lòng chọn loại tài khoản")]
        public string Role { get; set; } = "Candidate";

        // Thông tin công ty: bắt buộc kiểm tra trong controller
        // khi Role = Employer.
        public string? TenCongTy { get; set; }
        public string? DiaChiCongTy { get; set; }
        public string? SoDienThoaiCongTy { get; set; }
        public string? WebsiteCongTy { get; set; }
        public int? MaNganhNghe { get; set; }
        public int? MaQuyMo { get; set; }
        public string? GioiThieuCongTy { get; set; }
    }
}
