using Microsoft.EntityFrameworkCore;
using WorkConnect.Models.Entities;

namespace WorkConnect.Data;

/// <summary>
/// DbContext dùng chung cho cả hệ thống, cấu hình theo từ điển dữ liệu
/// ở mục 3.1.2 của báo cáo (Bảng 3.12 – 3.20).
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Nghiệp vụ
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<UngVien> UngViens => Set<UngVien>();
    public DbSet<CVUngVien> CVUngViens => Set<CVUngVien>();
    public DbSet<NhaTuyenDung> NhaTuyenDungs => Set<NhaTuyenDung>();
    public DbSet<TinTuyenDung> TinTuyenDungs => Set<TinTuyenDung>();
    public DbSet<DonUngTuyen> DonUngTuyens => Set<DonUngTuyen>();
    public DbSet<LichSuXuLyDon> LichSuXuLyDons => Set<LichSuXuLyDon>();

    // Danh mục
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<TrangThaiDon> TrangThaiDons => Set<TrangThaiDon>();
    public DbSet<NganhNghe> NganhNghes => Set<NganhNghe>();
    public DbSet<DiaDiem> DiaDiems => Set<DiaDiem>();
    public DbSet<TrinhDoHocVan> TrinhDoHocVans => Set<TrinhDoHocVan>();
    public DbSet<QuyMoCongTy> QuyMoCongTies => Set<QuyMoCongTy>();
    public DbSet<LoaiHinhCongViec> LoaiHinhCongViecs => Set<LoaiHinhCongViec>();
    public DbSet<HinhThucLamViec> HinhThucLamViecs => Set<HinhThucLamViec>();
    public DbSet<MucKinhNghiem> MucKinhNghiems => Set<MucKinhNghiem>();

    // Mẫu kiểm tra số điện thoại: 10 chữ số, bắt đầu bằng 0
    private const string PhoneRule = "{0} LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'";

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        ConfigureCatalogs(mb);
        ConfigureTaiKhoan(mb);
        ConfigureUngVien(mb);
        ConfigureCVUngVien(mb);
        ConfigureNhaTuyenDung(mb);
        ConfigureTinTuyenDung(mb);
        ConfigureDonUngTuyen(mb);
        ConfigureLichSuXuLyDon(mb);
        SeedCatalogs(mb);

        // Báo cáo chỉ quy định ON DELETE CASCADE cho UngVien -> CVUngVien (Bảng 3.14).
        // Các khóa ngoại còn lại để Restrict (cũng tránh lỗi "multiple cascade paths" của SQL Server).
        foreach (var fk in mb.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            if (fk.DeclaringEntityType.ClrType == typeof(CVUngVien)) continue;
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    // ---------- Danh mục (Bảng 3.19, 3.20) ----------
    private static void ConfigureCatalogs(ModelBuilder mb)
    {
        mb.Entity<VaiTro>(e => { e.ToTable("VaiTro"); e.Property(x => x.TenVaiTro).HasMaxLength(50).IsRequired(); e.HasIndex(x => x.TenVaiTro).IsUnique(); });
        mb.Entity<TrangThaiDon>(e => { e.ToTable("TrangThaiDon"); e.Property(x => x.TenTrangThai).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenTrangThai).IsUnique(); });
        mb.Entity<LoaiHinhCongViec>(e => { e.ToTable("LoaiHinhCongViec"); e.Property(x => x.TenLoaiHinh).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenLoaiHinh).IsUnique(); });
        mb.Entity<HinhThucLamViec>(e => { e.ToTable("HinhThucLamViec"); e.Property(x => x.TenHinhThuc).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenHinhThuc).IsUnique(); });
        mb.Entity<MucKinhNghiem>(e => { e.ToTable("MucKinhNghiem"); e.Property(x => x.TenMucKinhNghiem).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenMucKinhNghiem).IsUnique(); });
        mb.Entity<TrinhDoHocVan>(e => { e.ToTable("TrinhDoHocVan"); e.Property(x => x.TenTrinhDo).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenTrinhDo).IsUnique(); });
        mb.Entity<QuyMoCongTy>(e => { e.ToTable("QuyMoCongTy"); e.Property(x => x.TenQuyMo).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenQuyMo).IsUnique(); });
        mb.Entity<NganhNghe>(e => { e.ToTable("NganhNghe"); e.Property(x => x.TenNganhNghe).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenNganhNghe).IsUnique(); });
        mb.Entity<DiaDiem>(e => { e.ToTable("DiaDiem"); e.Property(x => x.TenDiaDiem).HasMaxLength(100).IsRequired(); e.HasIndex(x => x.TenDiaDiem).IsUnique(); });
    }

    // ---------- Bảng 3.12 ----------
    private static void ConfigureTaiKhoan(ModelBuilder mb)
    {
        mb.Entity<TaiKhoan>(e =>
        {
            e.ToTable("TaiKhoan", t => t.HasCheckConstraint("CK_TaiKhoan_SoDienThoai", string.Format(PhoneRule, "[SoDienThoai]")));
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.MatKhauMaHoa).HasMaxLength(255).IsRequired();
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.SoDienThoai).HasColumnType("varchar(10)").IsRequired();
            e.Property(x => x.BiKhoa).HasDefaultValue(false);
            e.Property(x => x.NgayTao).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.HasOne(x => x.VaiTro).WithMany().HasForeignKey(x => x.MaVaiTro);
        });
    }

    // ---------- Bảng 3.13 ----------
    private static void ConfigureUngVien(ModelBuilder mb)
    {
        mb.Entity<UngVien>(e =>
        {
            e.ToTable("UngVien", t =>
            {
                t.HasCheckConstraint("CK_UngVien_NgaySinh", "[NgaySinh] <= CAST(SYSDATETIME() AS DATE)");
                t.HasCheckConstraint("CK_UngVien_GioiTinh", "[GioiTinh] IS NULL OR [GioiTinh] IN (N'Nam', N'Nữ', N'Khác')");
                t.HasCheckConstraint("CK_UngVien_SoDienThoaiLienHe", string.Format(PhoneRule, "[SoDienThoaiLienHe]"));
            });
            e.HasKey(x => x.MaUngVien); 
            e.Property(x => x.AnhDaiDien).HasMaxLength(255);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.NgaySinh).HasColumnType("date");
            e.Property(x => x.GioiTinh).HasMaxLength(10);
            e.Property(x => x.EmailLienHe).HasMaxLength(150).IsRequired();
            e.Property(x => x.SoDienThoaiLienHe).HasColumnType("varchar(10)").IsRequired();
            e.Property(x => x.Truong).HasMaxLength(200).IsRequired();
            e.Property(x => x.ChuyenNganh).HasMaxLength(100).IsRequired();
            e.Property(x => x.KyNang).IsRequired();                 // NVARCHAR(MAX)
            e.Property(x => x.ChungChi).HasMaxLength(1000);
            e.Property(x => x.MucTieuNgheNghiep).HasMaxLength(1000);

            e.HasIndex(x => x.MaTaiKhoan).IsUnique();               // 1 tài khoản - 1 hồ sơ
            e.HasOne(x => x.TaiKhoan).WithOne(x => x.UngVien).HasForeignKey<UngVien>(x => x.MaTaiKhoan);
            e.HasOne(x => x.DiaDiem).WithMany().HasForeignKey(x => x.MaDiaDiem);
            e.HasOne(x => x.TrinhDo).WithMany().HasForeignKey(x => x.MaTrinhDo);
        });
    }

    // ---------- Bảng 3.14 ----------
    private static void ConfigureCVUngVien(ModelBuilder mb)
    {
        mb.Entity<CVUngVien>(e =>
        {
            e.ToTable("CVUngVien", t =>
            {
                t.HasCheckConstraint("CK_CVUngVien_DuongDanTep", "LOWER([DuongDanTep]) LIKE '%.pdf' OR LOWER([DuongDanTep]) LIKE '%.docx'");
                t.HasCheckConstraint("CK_CVUngVien_DungLuongKB", "[DungLuongKB] > 0 AND [DungLuongKB] <= 5120");
            });
            e.HasKey(x => x.MaCV);
            e.Property(x => x.TenTep).HasMaxLength(255).IsRequired();
            e.Property(x => x.DuongDanTep).HasMaxLength(255).IsRequired();
            e.Property(x => x.NgayTaiLen).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.HasOne(x => x.UngVien).WithMany(x => x.CVs).HasForeignKey(x => x.MaUngVien)
                .OnDelete(DeleteBehavior.Cascade);                  // ON DELETE CASCADE
        });
    }

    // ---------- Bảng 3.15 ----------
    private static void ConfigureNhaTuyenDung(ModelBuilder mb)
    {
        mb.Entity<NhaTuyenDung>(e =>
        {
            // Báo cáo ghi cột SoDienThoaiLienHe có CHECK nhưng chưa nêu quy tắc cụ thể nên chưa thêm CHECK cho cột này.
            e.ToTable("NhaTuyenDung", t => t.HasCheckConstraint("CK_NhaTuyenDung_Website",
                "[Website] IS NULL OR [Website] LIKE 'http://%' OR [Website] LIKE 'https://%'"));
            e.HasKey(x => x.MaNhaTuyenDung);
            e.Property(x => x.TenCongTy).HasMaxLength(200).IsRequired();
            e.Property(x => x.DuongDanLogo).HasMaxLength(255);
            e.Property(x => x.EmailLienHe).HasMaxLength(150).IsRequired();
            e.Property(x => x.SoDienThoaiLienHe).HasColumnType("varchar(15)");
            e.Property(x => x.DiaChi).HasMaxLength(255).IsRequired();
            e.Property(x => x.Website).HasMaxLength(255);
            e.Property(x => x.GioiThieu).IsRequired();

            e.HasIndex(x => x.MaTaiKhoan).IsUnique();
            e.HasOne(x => x.TaiKhoan).WithOne(x => x.NhaTuyenDung).HasForeignKey<NhaTuyenDung>(x => x.MaTaiKhoan);
            e.HasOne(x => x.NganhNghe).WithMany().HasForeignKey(x => x.MaNganhNghe);
            e.HasOne(x => x.QuyMo).WithMany().HasForeignKey(x => x.MaQuyMo);
        });
    }

    // ---------- Bảng 3.16 ----------
    private static void ConfigureTinTuyenDung(ModelBuilder mb)
    {
        mb.Entity<TinTuyenDung>(e =>
        {
            e.ToTable("TinTuyenDung", t =>
            {
                t.HasCheckConstraint("CK_TinTuyenDung_LuongToiDa", "[LuongToiDa] IS NULL OR [LuongToiThieu] IS NULL OR [LuongToiDa] >= [LuongToiThieu]");
                t.HasCheckConstraint("CK_TinTuyenDung_Luong", "[LuongThoaThuan] = 1 OR [LuongToiThieu] IS NOT NULL OR [LuongToiDa] IS NOT NULL");
                t.HasCheckConstraint("CK_TinTuyenDung_SoLuongTuyen", "[SoLuongTuyen] IS NULL OR [SoLuongTuyen] > 0");
                t.HasCheckConstraint("CK_TinTuyenDung_HanUngTuyen", "[HanUngTuyen] >= CAST([NgayDang] AS DATE)");
                t.HasCheckConstraint("CK_TinTuyenDung_TrangThai", "[TrangThai] IN (N'Bản nháp', N'Đang tuyển', N'Đã đóng')");
            });
            e.HasKey(x => x.MaTin);
            e.Property(x => x.TieuDe).HasMaxLength(200).IsRequired();
            e.Property(x => x.DuongDanThanThien).HasMaxLength(250).IsRequired();
            e.HasIndex(x => x.DuongDanThanThien).IsUnique();
            e.Property(x => x.ViTri).HasMaxLength(150).IsRequired();
            e.Property(x => x.LuongToiThieu).HasColumnType("decimal(18,0)");
            e.Property(x => x.LuongToiDa).HasColumnType("decimal(18,0)");
            e.Property(x => x.LuongThoaThuan).HasDefaultValue(false);
            e.Property(x => x.MoTaCongViec).IsRequired();
            e.Property(x => x.YeuCauCongViec).IsRequired();
            e.Property(x => x.YeuCauUngVien).IsRequired();
            e.Property(x => x.NgayDang).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.HanUngTuyen).HasColumnType("date");
            e.Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("Bản nháp");

            e.HasOne(x => x.NhaTuyenDung).WithMany(x => x.TinTuyenDungs).HasForeignKey(x => x.MaNhaTuyenDung);
            e.HasOne(x => x.NganhNghe).WithMany().HasForeignKey(x => x.MaNganhNghe);
            e.HasOne(x => x.LoaiHinh).WithMany().HasForeignKey(x => x.MaLoaiHinh);
            e.HasOne(x => x.HinhThuc).WithMany().HasForeignKey(x => x.MaHinhThuc);
            e.HasOne(x => x.DiaDiem).WithMany().HasForeignKey(x => x.MaDiaDiem);
            e.HasOne(x => x.MucKinhNghiem).WithMany().HasForeignKey(x => x.MaMucKinhNghiem);
        });
    }

    // ---------- Bảng 3.17 ----------
    private static void ConfigureDonUngTuyen(ModelBuilder mb)
    {
        mb.Entity<DonUngTuyen>(e =>
        {
            e.ToTable("DonUngTuyen", t => t.HasCheckConstraint("CK_DonUngTuyen_SoDienThoai", string.Format(PhoneRule, "[SoDienThoai]")));
            e.HasKey(x => x.MaDon);
            e.Property(x => x.HoTen).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.SoDienThoai).HasColumnType("varchar(10)").IsRequired();
            e.Property(x => x.ThuGioiThieu).HasMaxLength(2000);
            e.Property(x => x.NgayNop).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");
            e.Property(x => x.MaTrangThai).HasDefaultValue(1);       // 1 = "Đã gửi"

            // Mỗi ứng viên chỉ ứng tuyển một lần cho mỗi tin
            e.HasIndex(x => new { x.MaTin, x.MaUngVien }).IsUnique();

            e.HasOne(x => x.TinTuyenDung).WithMany(x => x.DonUngTuyens).HasForeignKey(x => x.MaTin);
            e.HasOne(x => x.UngVien).WithMany(x => x.DonUngTuyens).HasForeignKey(x => x.MaUngVien);
            e.HasOne(x => x.CV).WithMany().HasForeignKey(x => x.MaCV);
            e.HasOne(x => x.TrangThai).WithMany().HasForeignKey(x => x.MaTrangThai);
        });
    }

    // ---------- Bảng 3.18 ----------
    private static void ConfigureLichSuXuLyDon(ModelBuilder mb)
    {
        mb.Entity<LichSuXuLyDon>(e =>
        {
            // Trạng thái 3 = "Mời phỏng vấn": bắt buộc có thời gian và địa điểm phỏng vấn
            e.ToTable("LichSuXuLyDon", t => t.HasCheckConstraint("CK_LichSuXuLyDon_PhongVan",
                "[MaTrangThai] <> 3 OR ([ThoiGianPhongVan] IS NOT NULL AND [DiaDiemPhongVan] IS NOT NULL)"));
            e.HasKey(x => x.MaLichSu);
            e.Property(x => x.ThoiGianPhongVan).HasColumnType("datetime2");
            e.Property(x => x.DiaDiemPhongVan).HasMaxLength(255);
            e.Property(x => x.GhiChuNoiBo).HasMaxLength(1000);
            e.Property(x => x.PhanHoiUngVien).HasMaxLength(1000);
            e.Property(x => x.NgayCapNhat).HasColumnType("datetime2").HasDefaultValueSql("SYSDATETIME()");

            e.HasOne(x => x.DonUngTuyen).WithMany(x => x.LichSuXuLys).HasForeignKey(x => x.MaDon);
            e.HasOne(x => x.TrangThai).WithMany().HasForeignKey(x => x.MaTrangThai);
        });
    }

    // ---------- Dữ liệu danh mục ban đầu (Bảng 3.19, 3.20) ----------
    private static void SeedCatalogs(ModelBuilder mb)
    {
        mb.Entity<VaiTro>().HasData(
            new VaiTro { MaVaiTro = 1, TenVaiTro = "Admin" },
            new VaiTro { MaVaiTro = 2, TenVaiTro = "Employer" },
            new VaiTro { MaVaiTro = 3, TenVaiTro = "Candidate" });

        mb.Entity<TrangThaiDon>().HasData(
            new TrangThaiDon { MaTrangThai = 1, TenTrangThai = "Đã gửi" },
            new TrangThaiDon { MaTrangThai = 2, TenTrangThai = "Đang xem xét" },
            new TrangThaiDon { MaTrangThai = 3, TenTrangThai = "Mời phỏng vấn" },
            new TrangThaiDon { MaTrangThai = 4, TenTrangThai = "Đạt" },
            new TrangThaiDon { MaTrangThai = 5, TenTrangThai = "Từ chối" });

        mb.Entity<LoaiHinhCongViec>().HasData(
            new LoaiHinhCongViec { MaLoaiHinh = 1, TenLoaiHinh = "Thực tập" },
            new LoaiHinhCongViec { MaLoaiHinh = 2, TenLoaiHinh = "Bán thời gian" },
            new LoaiHinhCongViec { MaLoaiHinh = 3, TenLoaiHinh = "Toàn thời gian" },
            new LoaiHinhCongViec { MaLoaiHinh = 4, TenLoaiHinh = "Mới tốt nghiệp" });

        mb.Entity<HinhThucLamViec>().HasData(
            new HinhThucLamViec { MaHinhThuc = 1, TenHinhThuc = "Tại văn phòng" },
            new HinhThucLamViec { MaHinhThuc = 2, TenHinhThuc = "Từ xa" },
            new HinhThucLamViec { MaHinhThuc = 3, TenHinhThuc = "Kết hợp" });

        mb.Entity<MucKinhNghiem>().HasData(
            new MucKinhNghiem { MaMucKinhNghiem = 1, TenMucKinhNghiem = "Không yêu cầu" },
            new MucKinhNghiem { MaMucKinhNghiem = 2, TenMucKinhNghiem = "Dưới 1 năm" },
            new MucKinhNghiem { MaMucKinhNghiem = 3, TenMucKinhNghiem = "1-2 năm" },
            new MucKinhNghiem { MaMucKinhNghiem = 4, TenMucKinhNghiem = "Trên 2 năm" });

        mb.Entity<TrinhDoHocVan>().HasData(
            new TrinhDoHocVan { MaTrinhDo = 1, TenTrinhDo = "Sinh viên năm cuối" },
            new TrinhDoHocVan { MaTrinhDo = 2, TenTrinhDo = "Cao đẳng" },
            new TrinhDoHocVan { MaTrinhDo = 3, TenTrinhDo = "Đại học" },
            new TrinhDoHocVan { MaTrinhDo = 4, TenTrinhDo = "Sau đại học" });

        mb.Entity<QuyMoCongTy>().HasData(
            new QuyMoCongTy { MaQuyMo = 1, TenQuyMo = "Dưới 50 nhân viên" },
            new QuyMoCongTy { MaQuyMo = 2, TenQuyMo = "50-200 nhân viên" },
            new QuyMoCongTy { MaQuyMo = 3, TenQuyMo = "Trên 200 nhân viên" });

        mb.Entity<NganhNghe>().HasData(
            new NganhNghe { MaNganhNghe = 1, TenNganhNghe = "Công nghệ thông tin" },
            new NganhNghe { MaNganhNghe = 2, TenNganhNghe = "Kế toán - Kiểm toán" },
            new NganhNghe { MaNganhNghe = 3, TenNganhNghe = "Marketing" },
            new NganhNghe { MaNganhNghe = 4, TenNganhNghe = "Kinh doanh - Bán hàng" },
            new NganhNghe { MaNganhNghe = 5, TenNganhNghe = "Thiết kế - Sáng tạo" },
            new NganhNghe { MaNganhNghe = 6, TenNganhNghe = "Giáo dục - Đào tạo" },
            new NganhNghe { MaNganhNghe = 7, TenNganhNghe = "Nhân sự" },
            new NganhNghe { MaNganhNghe = 8, TenNganhNghe = "Logistics - Vận tải" });

        mb.Entity<DiaDiem>().HasData(
            new DiaDiem { MaDiaDiem = 1, TenDiaDiem = "Hà Nội" },
            new DiaDiem { MaDiaDiem = 2, TenDiaDiem = "TP. Hồ Chí Minh" },
            new DiaDiem { MaDiaDiem = 3, TenDiaDiem = "Đà Nẵng" },
            new DiaDiem { MaDiaDiem = 4, TenDiaDiem = "Hải Phòng" },
            new DiaDiem { MaDiaDiem = 5, TenDiaDiem = "Cần Thơ" });
    }
}