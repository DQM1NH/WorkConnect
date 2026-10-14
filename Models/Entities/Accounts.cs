using System.ComponentModel.DataAnnotations;

namespace WorkConnect.Models.Entities;

// Bảng 3.12. TaiKhoan
public class TaiKhoan
{
    [Key] public int MaTaiKhoan { get; set; }
    public int MaVaiTro { get; set; }
    public string Email { get; set; } = string.Empty;          // UNIQUE, dùng làm tên đăng nhập
    public string MatKhauMaHoa { get; set; } = string.Empty;   // mật khẩu đã băm
    public string HoTen { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;    // 10 chữ số, bắt đầu bằng 0
    public bool BiKhoa { get; set; }                           // 1 = bị khóa
    public DateTime NgayTao { get; set; }

    public VaiTro? VaiTro { get; set; }
    public UngVien? UngVien { get; set; }
    public NhaTuyenDung? NhaTuyenDung { get; set; }
}