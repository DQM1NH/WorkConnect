// Models/Entities/DonUngTuyen.cs
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
        public DateTime NgayNop { get; set; }
    }
}