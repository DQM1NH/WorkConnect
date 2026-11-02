namespace WorkConnect.Models.Entities;

// Bảng 3.13. UngVien
public class UngVien
{
    public int MaUngVien { get; set; }
    public int MaTaiKhoan { get; set; }
    public string? AnhDaiDien { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public DateTime NgaySinh { get; set; }                     // kiểu DATE
    public string? GioiTinh { get; set; }                      // Nam / Nữ / Khác
    public string EmailLienHe { get; set; } = string.Empty;
    public string SoDienThoaiLienHe { get; set; } = string.Empty;
    public int? MaDiaDiem { get; set; }
    public string Truong { get; set; } = string.Empty;
    public string ChuyenNganh { get; set; } = string.Empty;
    public int MaTrinhDo { get; set; }
    public int? NamTotNghiep { get; set; }
    public string KyNang { get; set; } = string.Empty;
    public string? KinhNghiem { get; set; }
    public string? ChungChi { get; set; }
    public string? MucTieuNgheNghiep { get; set; }

    public TaiKhoan? TaiKhoan { get; set; }
    public DiaDiem? DiaDiem { get; set; }
    public TrinhDoHocVan? TrinhDo { get; set; }
    public ICollection<CVUngVien> CVs { get; set; } = new List<CVUngVien>();
    public ICollection<DonUngTuyen> DonUngTuyens { get; set; } = new List<DonUngTuyen>();
}

// Bảng 3.14. CVUngVien
public class CVUngVien
{
    public int MaCV { get; set; }
    public int MaUngVien { get; set; }
    public string TenTep { get; set; } = string.Empty;
    public string DuongDanTep { get; set; } = string.Empty;    // chỉ .pdf hoặc .docx
    public int DungLuongKB { get; set; }                       // > 0 và <= 5120
    public DateTime NgayTaiLen { get; set; }

    public UngVien? UngVien { get; set; }
}

// Bảng 3.17. DonUngTuyen
public class DonUngTuyen
{
    public int MaDon { get; set; }
    public int MaTin { get; set; }
    public int MaUngVien { get; set; }
    public int MaCV { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string? ThuGioiThieu { get; set; }
    public DateTime NgayNop { get; set; }
    public int MaTrangThai { get; set; } = 1;                  // mặc định 1 = "Đã gửi"

    public TinTuyenDung? TinTuyenDung { get; set; }
    public UngVien? UngVien { get; set; }
    public CVUngVien? CV { get; set; }
    public TrangThaiDon? TrangThai { get; set; }
    public ICollection<LichSuXuLyDon> LichSuXuLys { get; set; } = new List<LichSuXuLyDon>();
}

// Bảng 3.18. LichSuXuLyDon
public class LichSuXuLyDon
{
    public int MaLichSu { get; set; }
    public int MaDon { get; set; }
    public int MaTrangThai { get; set; }
    public DateTime? ThoiGianPhongVan { get; set; }
    public string? DiaDiemPhongVan { get; set; }
    public string? GhiChuNoiBo { get; set; }                   // ứng viên KHÔNG được xem
    public string? PhanHoiUngVien { get; set; }                // hiển thị cho ứng viên
    public DateTime NgayCapNhat { get; set; }

    public DonUngTuyen? DonUngTuyen { get; set; }
    public TrangThaiDon? TrangThai { get; set; }
}