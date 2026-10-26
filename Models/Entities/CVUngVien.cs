using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_CVUngVien")]
    public class CVUngVien
    {
        [Key]
        public int MaCV { get; set; }
        public int MaUngVien { get; set; }
        public string TenTep { get; set; } = string.Empty;
        public string DuongDanTep { get; set; } = string.Empty;    // chỉ .pdf hoặc .docx
        public int DungLuongKB { get; set; }                       // > 0 và <= 5120
        public DateTime NgayTaiLen { get; set; }

        public UngVien? UngVien { get; set; }
    }
}