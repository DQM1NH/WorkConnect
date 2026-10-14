using Microsoft.EntityFrameworkCore;
using WorkConnect.Models.Entities;

namespace WorkConnect.Models
{
    public class WorkConnectDbContext : DbContext
    {
        public WorkConnectDbContext(DbContextOptions<WorkConnectDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<TinTuyenDung> TinTuyenDungs { get; set; }
        public DbSet<DonUngTuyen> DonUngTuyens { get; set; }
        public DbSet<NhaTuyenDung> NhaTuyenDungs { get; set; }
        public DbSet<NganhNghe> NganhNghes { get; set; }
        public DbSet<DiaDiem> DiaDiems { get; set; }
    }
}