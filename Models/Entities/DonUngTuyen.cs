using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_DonUngTuyen")]
    public class DonUngTuyen
    {
        [Key]
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
}