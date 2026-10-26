using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_NhaTuyenDung")]
    public class NhaTuyenDung
    {
        [Key]
        public int MaNhaTuyenDung { get; set; }

        [Required]
        public string TenCongTy { get; set; }

        public string? DuongDanLogo { get; set; }
        public string DiaChi { get; set; }
        public int MaTaiKhoan { get; set; }
        public string EmailLienHe { get; set; } = string.Empty;
        public string? SoDienThoaiLienHe { get; set; }
        public string? Website { get; set; }
        public int MaNganhNghe { get; set; }
        public int? MaQuyMo { get; set; }
        public string GioiThieu { get; set; } = string.Empty;

        public TaiKhoan? TaiKhoan { get; set; }
        public NganhNghe? NganhNghe { get; set; }
        public QuyMoCongTy? QuyMo { get; set; }
        public ICollection<TinTuyenDung> TinTuyenDungs { get; set; }
    }
}