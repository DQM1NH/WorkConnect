using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkConnect.Models.Entities
{
    [Table("tbl_NganhNghe")]
    public class NganhNghe
    {
        [Key]
        public int MaNganhNghe { get; set; }
        public string TenNganhNghe { get; set; }
    }

    [Table("tbl_DiaDiem")]
    public class DiaDiem
    {
        [Key]
        public int MaDiaDiem { get; set; }
        public string TenDiaDiem { get; set; }
    }
}