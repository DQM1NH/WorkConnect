using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_LichSuXuLyDon")]
    public class LichSuXuLyDon
    {
        [Key]
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
}