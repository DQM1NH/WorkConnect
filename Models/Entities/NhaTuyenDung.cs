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

        public string DuongDanLogo { get; set; }
        public string DiaChi { get; set; }
    }
}