using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_TinTuyenDung")]
    public class TinTuyenDung
    {
        [Key]
        public int MaTin { get; set; }

        [Required]
        public int MaNhaTuyenDung { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; }

        public string DuongDanThanThien { get; set; }
        public string ViTri { get; set; }

        public int MaNganhNghe { get; set; }
        public int MaLoaiHinh { get; set; }
        public int MaHinhThuc { get; set; }
        public int MaDiaDiem { get; set; }
        public int? MaMucKinhNghiem { get; set; }

        public decimal? LuongToiThieu { get; set; }
        public decimal? LuongToiDa { get; set; }
        public bool LuongThoaThuan { get; set; }
        public int? SoLuongTuyen { get; set; }  

        public string MoTaCongViec { get; set; }
        public string YeuCauCongViec { get; set; }
        public DateTime NgayDang { get; set; }
        public DateTime HanUngTuyen { get; set; }
        public string TrangThai { get; set; }
        public bool BiAn { get; set; } 

        // Navigation Properties (Quan hệ khóa ngoại)
        [ForeignKey("MaNhaTuyenDung")]
        public virtual NhaTuyenDung NhaTuyenDung { get; set; }

        [ForeignKey("MaNganhNghe")]
        public virtual NganhNghe NganhNghe { get; set; }

        [ForeignKey("MaDiaDiem")]
        public virtual DiaDiem DiaDiem { get; set; }
    }
}