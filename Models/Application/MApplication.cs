using System.ComponentModel.DataAnnotations;

namespace WorkConnect.Models;

/// <summary>BM05 - Đơn ứng tuyển (UC07).</summary>
public class ApplyForm
{
    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
    public string HoTen { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")]
    public string SoDienThoai { get; set; } = string.Empty;

    public int? MaCV { get; set; }                       // CV có sẵn được chọn

    [Display(Name = "Thư giới thiệu")]
    [StringLength(2000, ErrorMessage = "Thư giới thiệu tối đa 2000 ký tự")]
    public string? ThuGioiThieu { get; set; }
}

public class ApplyViewModel
{
    public int MaTin { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string TenCongTy { get; set; } = string.Empty;
    public DateTime HanUngTuyen { get; set; }
    public string TrangThaiTin { get; set; } = string.Empty;
    public ApplyForm Form { get; set; } = new();
    public List<CvItem> Cvs { get; set; } = new();

    public bool ConNhanDon => TrangThaiTin == "Đang tuyển" && HanUngTuyen.Date >= DateTime.Today;
}

public enum ApplyCheck { Ready, JobNotFound, JobClosed, AlreadyApplied, NoProfile, Invalid }

public record ApplyPrepareResult(ApplyCheck Check, ApplyViewModel? Model = null);

public record ApplySubmitResult(ApplyCheck Check, string? Error = null, int MaDon = 0)
{
    public bool Ok => Check == ApplyCheck.Ready;
}

/// <summary>UC08 - Đơn ứng tuyển hiển thị cho ứng viên (KHÔNG có GhiChuNoiBo).</summary>
public class ApplicationItem
{
    public int MaDon { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string TenCongTy { get; set; } = string.Empty;
    public string TenCV { get; set; } = string.Empty;
    public DateTime NgayNop { get; set; }
    public int MaTrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public List<ApplicationHistoryItem> LichSu { get; set; } = new();
}

public class ApplicationHistoryItem
{
    public int MaTrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
    public DateTime? ThoiGianPhongVan { get; set; }
    public string? DiaDiemPhongVan { get; set; }
    public string? PhanHoiUngVien { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class ApplicationsViewModel
{
    public List<ApplicationItem> Items { get; set; } = new();
    public List<OptionItem> Statuses { get; set; } = new();
    public int? Status { get; set; }
}