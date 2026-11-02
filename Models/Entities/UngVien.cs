using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_UngVien")]
    public class UngVien
    {
        [Key]
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
}