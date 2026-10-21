using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_TaiKhoan")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }
        public int MaVaiTro { get; set; }
        public string Email { get; set; } = string.Empty;

        /// <summary>Cột MatKhauMaHoa trong CSDL: lưu mật khẩu đã băm, KHÔNG lưu mật khẩu gốc.</summary>
        [Column("MatKhauMaHoa")]
        public string MatKhauHash { get; set; } = string.Empty;

        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public bool BiKhoa { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
