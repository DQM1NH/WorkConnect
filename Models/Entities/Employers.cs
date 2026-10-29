namespace WorkConnect.Models.Entities;

// Bảng 3.15. NhaTuyenDung
public class NhaTuyenDung
{
    public int MaNhaTuyenDung { get; set; }
    public int MaTaiKhoan { get; set; }
    public string TenCongTy { get; set; } = string.Empty;
    public string? DuongDanLogo { get; set; }
    public string EmailLienHe { get; set; } = string.Empty;
    public string? SoDienThoaiLienHe { get; set; }
    public string DiaChi { get; set; } = string.Empty;
    public string? Website { get; set; }
    public int MaNganhNghe { get; set; }
    public int? MaQuyMo { get; set; }
    public string GioiThieu { get; set; } = string.Empty;

    public TaiKhoan? TaiKhoan { get; set; }
    public NganhNghe? NganhNghe { get; set; }
    public QuyMoCongTy? QuyMo { get; set; }
    public ICollection<TinTuyenDung> TinTuyenDungs { get; set; } = new List<TinTuyenDung>();
}

// Bảng 3.16. TinTuyenDung
public class TinTuyenDung
{
    public int MaTin { get; set; }
    public int MaNhaTuyenDung { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string DuongDanThanThien { get; set; } = string.Empty;
    public string ViTri { get; set; } = string.Empty;
    public int MaNganhNghe { get; set; }
    public int MaLoaiHinh { get; set; }
    public int MaHinhThuc { get; set; }
    public int MaDiaDiem { get; set; }
    public int? MaMucKinhNghiem { get; set; }
    public decimal? LuongToiThieu { get; set; }
    public decimal? LuongToiDa { get; set; }
    public bool LuongThoaThuan { get; set; }
    public int? SoLuongTuyen { get; set; }
    public string MoTaCongViec { get; set; } = string.Empty;
    public string YeuCauCongViec { get; set; } = string.Empty;
    public string YeuCauUngVien { get; set; } = string.Empty;
    public string? QuyenLoi { get; set; }
    public DateTime NgayDang { get; set; }
    public DateTime HanUngTuyen { get; set; }                  // kiểu DATE
    public string TrangThai { get; set; } = "Đang tuyển";        // Đang tuyển / Đã đóng / Đã hết hạn

    public NhaTuyenDung? NhaTuyenDung { get; set; }
    public NganhNghe? NganhNghe { get; set; }
    public LoaiHinhCongViec? LoaiHinh { get; set; }
    public HinhThucLamViec? HinhThuc { get; set; }
    public DiaDiem? DiaDiem { get; set; }
    public MucKinhNghiem? MucKinhNghiem { get; set; }
    public ICollection<DonUngTuyen> DonUngTuyens { get; set; } = new List<DonUngTuyen>();
}