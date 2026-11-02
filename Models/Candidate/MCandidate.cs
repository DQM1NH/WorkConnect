using System.ComponentModel.DataAnnotations;

namespace WorkConnect.Models;

public record ServiceResult(bool Ok, string? Error = null);
public record ServiceResult<T>(bool Ok, T? Data = default, string? Error = null);
public record StoredFile(string PhysicalPath, string ContentType, string DownloadName);

/// <summary>BM02 - Hồ sơ ứng viên (UC03).</summary>
public class ProfileForm : IValidatableObject
{
    private static readonly string[] GioiTinhHopLe = { "Nam", "Nữ", "Khác" };

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
    public string HoTen { get; set; } = string.Empty;

    [Display(Name = "Ngày sinh")]
    [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
    [DataType(DataType.Date)]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [Display(Name = "Email liên hệ")]
    [Required(ErrorMessage = "Vui lòng nhập email liên hệ")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
    public string EmailLienHe { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
    public string SoDienThoaiLienHe { get; set; } = string.Empty;

    [Display(Name = "Tỉnh/Thành phố")]
    public int? MaDiaDiem { get; set; }

    [Display(Name = "Trường")]
    [Required(ErrorMessage = "Vui lòng nhập tên trường")]
    [StringLength(200, ErrorMessage = "Tên trường tối đa 200 ký tự")]
    public string Truong { get; set; } = string.Empty;

    [Display(Name = "Chuyên ngành")]
    [Required(ErrorMessage = "Vui lòng nhập chuyên ngành")]
    [StringLength(100, ErrorMessage = "Chuyên ngành tối đa 100 ký tự")]
    public string ChuyenNganh { get; set; } = string.Empty;

    [Display(Name = "Trình độ học vấn")]
    [Required(ErrorMessage = "Vui lòng chọn trình độ học vấn")]
    public int? MaTrinhDo { get; set; }

    [Display(Name = "Năm tốt nghiệp")]
    public int? NamTotNghiep { get; set; }

    [Display(Name = "Kỹ năng")]
    [Required(ErrorMessage = "Vui lòng nhập kỹ năng")]
    [StringLength(2000, ErrorMessage = "Kỹ năng tối đa 2000 ký tự")]
    public string KyNang { get; set; } = string.Empty;

    [Display(Name = "Kinh nghiệm / Dự án")]
    [StringLength(4000, ErrorMessage = "Tối đa 4000 ký tự")]
    public string? KinhNghiem { get; set; }

    [Display(Name = "Chứng chỉ")]
    [StringLength(1000, ErrorMessage = "Chứng chỉ tối đa 1000 ký tự")]
    public string? ChungChi { get; set; }

    [Display(Name = "Mục tiêu nghề nghiệp")]
    [StringLength(1000, ErrorMessage = "Mục tiêu tối đa 1000 ký tự")]
    public string? MucTieuNgheNghiep { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NgaySinh.HasValue)
        {
            if (NgaySinh.Value.Date > DateTime.Today)
                yield return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại", new[] { nameof(NgaySinh) });
            else if (NgaySinh.Value.Year < 1900)
                yield return new ValidationResult("Ngày sinh không hợp lệ", new[] { nameof(NgaySinh) });
        }
        if (!string.IsNullOrEmpty(GioiTinh) && !GioiTinhHopLe.Contains(GioiTinh))
            yield return new ValidationResult("Giới tính không hợp lệ", new[] { nameof(GioiTinh) });
        if (NamTotNghiep is int nam && (nam < 1990 || nam > DateTime.Today.Year + 10))
            yield return new ValidationResult("Năm tốt nghiệp không hợp lệ", new[] { nameof(NamTotNghiep) });
    }
}

public class ProfileOptions
{
    public List<OptionItem> DiaDiems { get; set; } = new();
    public List<OptionItem> TrinhDos { get; set; } = new();
}

public class ProfileViewModel
{
    public ProfileForm Form { get; set; } = new();
    public ProfileOptions Options { get; set; } = new();
    public bool DaCoHoSo { get; set; }
    public bool CoAnhDaiDien { get; set; }
    public string? ReturnUrl { get; set; }
}

/// <summary>UC04 - Một CV của ứng viên.</summary>
public class CvItem
{
    public int MaCV { get; set; }
    public string TenTep { get; set; } = string.Empty;
    public int DungLuongKB { get; set; }
    public DateTime NgayTaiLen { get; set; }
    public bool DaDungUngTuyen { get; set; }      // đã gắn với đơn ứng tuyển -> không được xóa
}

public class CvPageViewModel
{
    public List<CvItem> Cvs { get; set; } = new();
    public bool DaCoHoSo { get; set; }
}