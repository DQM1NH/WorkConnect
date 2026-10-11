using System.Globalization;

namespace WorkConnect.Models
{
    /// <summary>Các kiểu sắp xếp theo BM07: Mới nhất / Lương cao nhất / Sắp hết hạn.</summary>
    public static class JobSort
    {
        public const string MoiNhat = "moi-nhat";
        public const string LuongCao = "luong-cao";
        public const string SapHetHan = "sap-het-han";
        public static readonly string[] All = { MoiNhat, LuongCao, SapHetHan };
    }

    /// <summary>Từ khóa và bộ lọc của BM07. Tất cả đều không bắt buộc.</summary>
    public class JobSearchFilter
    {
        public string? Keyword { get; set; }          // tiêu đề, vị trí hoặc tên công ty
        public int? MaNganhNghe { get; set; }
        public int? MaDiaDiem { get; set; }
        public int? MaLoaiHinh { get; set; }
        public int? MaMucKinhNghiem { get; set; }
        public int? MaHinhThuc { get; set; }
        public decimal? LuongTuTrieu { get; set; }    // đơn vị: triệu đồng
        public decimal? LuongDenTrieu { get; set; }   // đơn vị: triệu đồng
        public string Sort { get; set; } = JobSort.MoiNhat;

        /// <summary>Làm sạch dữ liệu đầu vào do người dùng gửi lên.</summary>
        public void Normalize()
        {
            Keyword = string.IsNullOrWhiteSpace(Keyword) ? null : Keyword.Trim();
            if (Keyword != null && Keyword.Length > 100) Keyword = Keyword.Substring(0, 100);

            LuongTuTrieu = ClampTrieu(LuongTuTrieu);
            LuongDenTrieu = ClampTrieu(LuongDenTrieu);
            if (LuongTuTrieu.HasValue && LuongDenTrieu.HasValue && LuongTuTrieu > LuongDenTrieu)
                (LuongTuTrieu, LuongDenTrieu) = (LuongDenTrieu, LuongTuTrieu);

            if (string.IsNullOrEmpty(Sort) || !JobSort.All.Contains(Sort)) Sort = JobSort.MoiNhat;
        }

        private static decimal? ClampTrieu(decimal? v)
        {
            if (!v.HasValue || v.Value < 0) return null;
            return Math.Min(v.Value, 100000m);        // chặn số quá lớn gây tràn kiểu decimal
        }
    }

    public record OptionItem(int Id, string Name);

    /// <summary>Dữ liệu các ô chọn lấy từ bảng danh mục.</summary>
    public class JobFilterOptions
    {
        public List<OptionItem> NganhNghes { get; set; } = new();
        public List<OptionItem> DiaDiems { get; set; } = new();
        public List<OptionItem> LoaiHinhs { get; set; } = new();
        public List<OptionItem> MucKinhNghiems { get; set; } = new();
        public List<OptionItem> HinhThucs { get; set; } = new();
    }

    public class JobListItem
    {
        public int MaTin { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string DuongDanThanThien { get; set; } = string.Empty;
        public string ViTri { get; set; } = string.Empty;
        public string TenCongTy { get; set; } = string.Empty;
        public string? DuongDanLogo { get; set; }
        public string DiaDiem { get; set; } = string.Empty;
        public string NganhNghe { get; set; } = string.Empty;
        public string LoaiHinh { get; set; } = string.Empty;
        public string HinhThuc { get; set; } = string.Empty;
        public string? KinhNghiem { get; set; }
        public decimal? LuongToiThieu { get; set; }
        public decimal? LuongToiDa { get; set; }
        public bool LuongThoaThuan { get; set; }
        public DateTime HanUngTuyen { get; set; }

        public string LuongHienThi => JobFormat.Luong(LuongToiThieu, LuongToiDa, LuongThoaThuan);
    }

    /// <summary>Model của trang danh sách việc làm (lần hiển thị đầu tiên).</summary>
    public class JobSearchViewModel
    {
        public JobSearchFilter Filter { get; set; } = new();
        public JobFilterOptions Options { get; set; } = new();
        public List<JobListItem> Items { get; set; } = new();
    }

    /// <summary>Thông tin chi tiết tin tuyển dụng (R05).</summary>
    public class JobDetailViewModel
    {
        public int MaTin { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string ViTri { get; set; } = string.Empty;
        public string NganhNghe { get; set; } = string.Empty;
        public string DiaDiem { get; set; } = string.Empty;
        public string LoaiHinh { get; set; } = string.Empty;
        public string HinhThuc { get; set; } = string.Empty;
        public string? KinhNghiem { get; set; }
        public decimal? LuongToiThieu { get; set; }
        public decimal? LuongToiDa { get; set; }
        public bool LuongThoaThuan { get; set; }
        public int? SoLuongTuyen { get; set; }
        public string MoTaCongViec { get; set; } = string.Empty;
        public string YeuCauCongViec { get; set; } = string.Empty;
        public string YeuCauUngVien { get; set; } = string.Empty;
        public string? QuyenLoi { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime HanUngTuyen { get; set; }
        public string TrangThai { get; set; } = string.Empty;

        // Thông tin doanh nghiệp (BM03)
        public string TenCongTy { get; set; } = string.Empty;
        public string? DuongDanLogo { get; set; }
        public string DiaChiCongTy { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string GioiThieuCongTy { get; set; } = string.Empty;

        public string LuongHienThi => JobFormat.Luong(LuongToiThieu, LuongToiDa, LuongThoaThuan);

        /// <summary>Tin đang tuyển và chưa quá hạn ứng tuyển.</summary>
        public bool ConNhanDon => TrangThai == "Đang tuyển" && HanUngTuyen.Date >= DateTime.Today;
    }

    public static class JobFormat
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        public static string Luong(decimal? min, decimal? max, bool thoaThuan)
        {
            if (thoaThuan || (min is null && max is null)) return "Thỏa thuận";
            if (min.HasValue && max.HasValue)
                return min == max ? Tien(min.Value) : $"{Tien(min.Value)} - {Tien(max.Value)}";
            return min.HasValue ? $"Từ {Tien(min.Value)}" : $"Tới {Tien(max!.Value)}";
        }

        private static string Tien(decimal v) =>
            v >= 1_000_000m
                ? string.Format(Vi, "{0:0.##} triệu", v / 1_000_000m)
                : string.Format(Vi, "{0:N0} đ", v);
    }
}