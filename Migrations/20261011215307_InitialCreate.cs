using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WorkConnect.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiaDiem",
                columns: table => new
                {
                    MaDiaDiem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDiaDiem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaDiem", x => x.MaDiaDiem);
                });

            migrationBuilder.CreateTable(
                name: "HinhThucLamViec",
                columns: table => new
                {
                    MaHinhThuc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenHinhThuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhThucLamViec", x => x.MaHinhThuc);
                });

            migrationBuilder.CreateTable(
                name: "LoaiHinhCongViec",
                columns: table => new
                {
                    MaLoaiHinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiHinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiHinhCongViec", x => x.MaLoaiHinh);
                });

            migrationBuilder.CreateTable(
                name: "MucKinhNghiem",
                columns: table => new
                {
                    MaMucKinhNghiem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMucKinhNghiem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MucKinhNghiem", x => x.MaMucKinhNghiem);
                });

            migrationBuilder.CreateTable(
                name: "NganhNghe",
                columns: table => new
                {
                    MaNganhNghe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNganhNghe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NganhNghe", x => x.MaNganhNghe);
                });

            migrationBuilder.CreateTable(
                name: "QuyMoCongTy",
                columns: table => new
                {
                    MaQuyMo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenQuyMo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyMoCongTy", x => x.MaQuyMo);
                });

            migrationBuilder.CreateTable(
                name: "TrangThaiDon",
                columns: table => new
                {
                    MaTrangThai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTrangThai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrangThaiDon", x => x.MaTrangThai);
                });

            migrationBuilder.CreateTable(
                name: "TrinhDoHocVan",
                columns: table => new
                {
                    MaTrinhDo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTrinhDo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrinhDoHocVan", x => x.MaTrinhDo);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MatKhauMaHoa = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(10)", nullable: false),
                    BiKhoa = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                    table.CheckConstraint("CK_TaiKhoan_SoDienThoai", "[SoDienThoai] LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                    table.ForeignKey(
                        name: "FK_TaiKhoan_VaiTro_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhaTuyenDung",
                columns: table => new
                {
                    MaNhaTuyenDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    TenCongTy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DuongDanLogo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EmailLienHe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SoDienThoaiLienHe = table.Column<string>(type: "varchar(15)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaNganhNghe = table.Column<int>(type: "int", nullable: false),
                    MaQuyMo = table.Column<int>(type: "int", nullable: true),
                    GioiThieu = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaTuyenDung", x => x.MaNhaTuyenDung);
                    table.CheckConstraint("CK_NhaTuyenDung_Website", "[Website] IS NULL OR [Website] LIKE 'http://%' OR [Website] LIKE 'https://%'");
                    table.ForeignKey(
                        name: "FK_NhaTuyenDung_NganhNghe_MaNganhNghe",
                        column: x => x.MaNganhNghe,
                        principalTable: "NganhNghe",
                        principalColumn: "MaNganhNghe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhaTuyenDung_QuyMoCongTy_MaQuyMo",
                        column: x => x.MaQuyMo,
                        principalTable: "QuyMoCongTy",
                        principalColumn: "MaQuyMo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NhaTuyenDung_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UngVien",
                columns: table => new
                {
                    MaUngVien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EmailLienHe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SoDienThoaiLienHe = table.Column<string>(type: "varchar(10)", nullable: false),
                    MaDiaDiem = table.Column<int>(type: "int", nullable: true),
                    Truong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChuyenNganh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaTrinhDo = table.Column<int>(type: "int", nullable: false),
                    NamTotNghiep = table.Column<int>(type: "int", nullable: true),
                    KyNang = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KinhNghiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChungChi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MucTieuNgheNghiep = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UngVien", x => x.MaUngVien);
                    table.CheckConstraint("CK_UngVien_GioiTinh", "[GioiTinh] IS NULL OR [GioiTinh] IN (N'Nam', N'Nữ', N'Khác')");
                    table.CheckConstraint("CK_UngVien_NgaySinh", "[NgaySinh] <= CAST(SYSDATETIME() AS DATE)");
                    table.CheckConstraint("CK_UngVien_SoDienThoaiLienHe", "[SoDienThoaiLienHe] LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                    table.ForeignKey(
                        name: "FK_UngVien_DiaDiem_MaDiaDiem",
                        column: x => x.MaDiaDiem,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UngVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UngVien_TrinhDoHocVan_MaTrinhDo",
                        column: x => x.MaTrinhDo,
                        principalTable: "TrinhDoHocVan",
                        principalColumn: "MaTrinhDo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TinTuyenDung",
                columns: table => new
                {
                    MaTin = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNhaTuyenDung = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DuongDanThanThien = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ViTri = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MaNganhNghe = table.Column<int>(type: "int", nullable: false),
                    MaLoaiHinh = table.Column<int>(type: "int", nullable: false),
                    MaHinhThuc = table.Column<int>(type: "int", nullable: false),
                    MaDiaDiem = table.Column<int>(type: "int", nullable: false),
                    MaMucKinhNghiem = table.Column<int>(type: "int", nullable: true),
                    LuongToiThieu = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    LuongToiDa = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    LuongThoaThuan = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoLuongTuyen = table.Column<int>(type: "int", nullable: true),
                    MoTaCongViec = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YeuCauCongViec = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YeuCauUngVien = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuyenLoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    HanUngTuyen = table.Column<DateTime>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Bản nháp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TinTuyenDung", x => x.MaTin);
                    table.CheckConstraint("CK_TinTuyenDung_HanUngTuyen", "[HanUngTuyen] >= CAST([NgayDang] AS DATE)");
                    table.CheckConstraint("CK_TinTuyenDung_Luong", "[LuongThoaThuan] = 1 OR [LuongToiThieu] IS NOT NULL OR [LuongToiDa] IS NOT NULL");
                    table.CheckConstraint("CK_TinTuyenDung_LuongToiDa", "[LuongToiDa] IS NULL OR [LuongToiThieu] IS NULL OR [LuongToiDa] >= [LuongToiThieu]");
                    table.CheckConstraint("CK_TinTuyenDung_SoLuongTuyen", "[SoLuongTuyen] IS NULL OR [SoLuongTuyen] > 0");
                    table.CheckConstraint("CK_TinTuyenDung_TrangThai", "[TrangThai] IN (N'Bản nháp', N'Đang tuyển', N'Đã đóng')");
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_DiaDiem_MaDiaDiem",
                        column: x => x.MaDiaDiem,
                        principalTable: "DiaDiem",
                        principalColumn: "MaDiaDiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_HinhThucLamViec_MaHinhThuc",
                        column: x => x.MaHinhThuc,
                        principalTable: "HinhThucLamViec",
                        principalColumn: "MaHinhThuc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_LoaiHinhCongViec_MaLoaiHinh",
                        column: x => x.MaLoaiHinh,
                        principalTable: "LoaiHinhCongViec",
                        principalColumn: "MaLoaiHinh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_MucKinhNghiem_MaMucKinhNghiem",
                        column: x => x.MaMucKinhNghiem,
                        principalTable: "MucKinhNghiem",
                        principalColumn: "MaMucKinhNghiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_NganhNghe_MaNganhNghe",
                        column: x => x.MaNganhNghe,
                        principalTable: "NganhNghe",
                        principalColumn: "MaNganhNghe",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TinTuyenDung_NhaTuyenDung_MaNhaTuyenDung",
                        column: x => x.MaNhaTuyenDung,
                        principalTable: "NhaTuyenDung",
                        principalColumn: "MaNhaTuyenDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CVUngVien",
                columns: table => new
                {
                    MaCV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaUngVien = table.Column<int>(type: "int", nullable: false),
                    TenTep = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DuongDanTep = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DungLuongKB = table.Column<int>(type: "int", nullable: false),
                    NgayTaiLen = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVUngVien", x => x.MaCV);
                    table.CheckConstraint("CK_CVUngVien_DungLuongKB", "[DungLuongKB] > 0 AND [DungLuongKB] <= 5120");
                    table.CheckConstraint("CK_CVUngVien_DuongDanTep", "LOWER([DuongDanTep]) LIKE '%.pdf' OR LOWER([DuongDanTep]) LIKE '%.docx'");
                    table.ForeignKey(
                        name: "FK_CVUngVien_UngVien_MaUngVien",
                        column: x => x.MaUngVien,
                        principalTable: "UngVien",
                        principalColumn: "MaUngVien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonUngTuyen",
                columns: table => new
                {
                    MaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTin = table.Column<int>(type: "int", nullable: false),
                    MaUngVien = table.Column<int>(type: "int", nullable: false),
                    MaCV = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SoDienThoai = table.Column<string>(type: "varchar(10)", nullable: false),
                    ThuGioiThieu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NgayNop = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    MaTrangThai = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonUngTuyen", x => x.MaDon);
                    table.CheckConstraint("CK_DonUngTuyen_SoDienThoai", "[SoDienThoai] LIKE '0[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                    table.ForeignKey(
                        name: "FK_DonUngTuyen_CVUngVien_MaCV",
                        column: x => x.MaCV,
                        principalTable: "CVUngVien",
                        principalColumn: "MaCV",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonUngTuyen_TinTuyenDung_MaTin",
                        column: x => x.MaTin,
                        principalTable: "TinTuyenDung",
                        principalColumn: "MaTin",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonUngTuyen_TrangThaiDon_MaTrangThai",
                        column: x => x.MaTrangThai,
                        principalTable: "TrangThaiDon",
                        principalColumn: "MaTrangThai",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonUngTuyen_UngVien_MaUngVien",
                        column: x => x.MaUngVien,
                        principalTable: "UngVien",
                        principalColumn: "MaUngVien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuXuLyDon",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDon = table.Column<int>(type: "int", nullable: false),
                    MaTrangThai = table.Column<int>(type: "int", nullable: false),
                    ThoiGianPhongVan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiaDiemPhongVan = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GhiChuNoiBo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PhanHoiUngVien = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuXuLyDon", x => x.MaLichSu);
                    table.CheckConstraint("CK_LichSuXuLyDon_PhongVan", "[MaTrangThai] <> 3 OR ([ThoiGianPhongVan] IS NOT NULL AND [DiaDiemPhongVan] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_LichSuXuLyDon_DonUngTuyen_MaDon",
                        column: x => x.MaDon,
                        principalTable: "DonUngTuyen",
                        principalColumn: "MaDon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuXuLyDon_TrangThaiDon_MaTrangThai",
                        column: x => x.MaTrangThai,
                        principalTable: "TrangThaiDon",
                        principalColumn: "MaTrangThai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DiaDiem",
                columns: new[] { "MaDiaDiem", "TenDiaDiem" },
                values: new object[,]
                {
                    { 1, "Hà Nội" },
                    { 2, "TP. Hồ Chí Minh" },
                    { 3, "Đà Nẵng" },
                    { 4, "Hải Phòng" },
                    { 5, "Cần Thơ" }
                });

            migrationBuilder.InsertData(
                table: "HinhThucLamViec",
                columns: new[] { "MaHinhThuc", "TenHinhThuc" },
                values: new object[,]
                {
                    { 1, "Tại văn phòng" },
                    { 2, "Từ xa" },
                    { 3, "Kết hợp" }
                });

            migrationBuilder.InsertData(
                table: "LoaiHinhCongViec",
                columns: new[] { "MaLoaiHinh", "TenLoaiHinh" },
                values: new object[,]
                {
                    { 1, "Thực tập" },
                    { 2, "Bán thời gian" },
                    { 3, "Toàn thời gian" },
                    { 4, "Mới tốt nghiệp" }
                });

            migrationBuilder.InsertData(
                table: "MucKinhNghiem",
                columns: new[] { "MaMucKinhNghiem", "TenMucKinhNghiem" },
                values: new object[,]
                {
                    { 1, "Không yêu cầu" },
                    { 2, "Dưới 1 năm" },
                    { 3, "1-2 năm" },
                    { 4, "Trên 2 năm" }
                });

            migrationBuilder.InsertData(
                table: "NganhNghe",
                columns: new[] { "MaNganhNghe", "TenNganhNghe" },
                values: new object[,]
                {
                    { 1, "Công nghệ thông tin" },
                    { 2, "Kế toán - Kiểm toán" },
                    { 3, "Marketing" },
                    { 4, "Kinh doanh - Bán hàng" },
                    { 5, "Thiết kế - Sáng tạo" },
                    { 6, "Giáo dục - Đào tạo" },
                    { 7, "Nhân sự" },
                    { 8, "Logistics - Vận tải" }
                });

            migrationBuilder.InsertData(
                table: "QuyMoCongTy",
                columns: new[] { "MaQuyMo", "TenQuyMo" },
                values: new object[,]
                {
                    { 1, "Dưới 50 nhân viên" },
                    { 2, "50-200 nhân viên" },
                    { 3, "Trên 200 nhân viên" }
                });

            migrationBuilder.InsertData(
                table: "TrangThaiDon",
                columns: new[] { "MaTrangThai", "TenTrangThai" },
                values: new object[,]
                {
                    { 1, "Đã gửi" },
                    { 2, "Đang xem xét" },
                    { 3, "Mời phỏng vấn" },
                    { 4, "Đạt" },
                    { 5, "Từ chối" }
                });

            migrationBuilder.InsertData(
                table: "TrinhDoHocVan",
                columns: new[] { "MaTrinhDo", "TenTrinhDo" },
                values: new object[,]
                {
                    { 1, "Sinh viên năm cuối" },
                    { 2, "Cao đẳng" },
                    { 3, "Đại học" },
                    { 4, "Sau đại học" }
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "MaVaiTro", "TenVaiTro" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Employer" },
                    { 3, "Candidate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVUngVien_MaUngVien",
                table: "CVUngVien",
                column: "MaUngVien");

            migrationBuilder.CreateIndex(
                name: "IX_DiaDiem_TenDiaDiem",
                table: "DiaDiem",
                column: "TenDiaDiem",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonUngTuyen_MaCV",
                table: "DonUngTuyen",
                column: "MaCV");

            migrationBuilder.CreateIndex(
                name: "IX_DonUngTuyen_MaTin_MaUngVien",
                table: "DonUngTuyen",
                columns: new[] { "MaTin", "MaUngVien" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonUngTuyen_MaTrangThai",
                table: "DonUngTuyen",
                column: "MaTrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_DonUngTuyen_MaUngVien",
                table: "DonUngTuyen",
                column: "MaUngVien");

            migrationBuilder.CreateIndex(
                name: "IX_HinhThucLamViec_TenHinhThuc",
                table: "HinhThucLamViec",
                column: "TenHinhThuc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyDon_MaDon",
                table: "LichSuXuLyDon",
                column: "MaDon");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXuLyDon_MaTrangThai",
                table: "LichSuXuLyDon",
                column: "MaTrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiHinhCongViec_TenLoaiHinh",
                table: "LoaiHinhCongViec",
                column: "TenLoaiHinh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MucKinhNghiem_TenMucKinhNghiem",
                table: "MucKinhNghiem",
                column: "TenMucKinhNghiem",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NganhNghe_TenNganhNghe",
                table: "NganhNghe",
                column: "TenNganhNghe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NhaTuyenDung_MaNganhNghe",
                table: "NhaTuyenDung",
                column: "MaNganhNghe");

            migrationBuilder.CreateIndex(
                name: "IX_NhaTuyenDung_MaQuyMo",
                table: "NhaTuyenDung",
                column: "MaQuyMo");

            migrationBuilder.CreateIndex(
                name: "IX_NhaTuyenDung_MaTaiKhoan",
                table: "NhaTuyenDung",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuyMoCongTy_TenQuyMo",
                table: "QuyMoCongTy",
                column: "TenQuyMo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Email",
                table: "TaiKhoan",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaVaiTro",
                table: "TaiKhoan",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_DuongDanThanThien",
                table: "TinTuyenDung",
                column: "DuongDanThanThien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaDiaDiem",
                table: "TinTuyenDung",
                column: "MaDiaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaHinhThuc",
                table: "TinTuyenDung",
                column: "MaHinhThuc");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaLoaiHinh",
                table: "TinTuyenDung",
                column: "MaLoaiHinh");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaMucKinhNghiem",
                table: "TinTuyenDung",
                column: "MaMucKinhNghiem");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaNganhNghe",
                table: "TinTuyenDung",
                column: "MaNganhNghe");

            migrationBuilder.CreateIndex(
                name: "IX_TinTuyenDung_MaNhaTuyenDung",
                table: "TinTuyenDung",
                column: "MaNhaTuyenDung");

            migrationBuilder.CreateIndex(
                name: "IX_TrangThaiDon_TenTrangThai",
                table: "TrangThaiDon",
                column: "TenTrangThai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrinhDoHocVan_TenTrinhDo",
                table: "TrinhDoHocVan",
                column: "TenTrinhDo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UngVien_MaDiaDiem",
                table: "UngVien",
                column: "MaDiaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_UngVien_MaTaiKhoan",
                table: "UngVien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UngVien_MaTrinhDo",
                table: "UngVien",
                column: "MaTrinhDo");

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_TenVaiTro",
                table: "VaiTro",
                column: "TenVaiTro",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuXuLyDon");

            migrationBuilder.DropTable(
                name: "DonUngTuyen");

            migrationBuilder.DropTable(
                name: "CVUngVien");

            migrationBuilder.DropTable(
                name: "TinTuyenDung");

            migrationBuilder.DropTable(
                name: "TrangThaiDon");

            migrationBuilder.DropTable(
                name: "UngVien");

            migrationBuilder.DropTable(
                name: "HinhThucLamViec");

            migrationBuilder.DropTable(
                name: "LoaiHinhCongViec");

            migrationBuilder.DropTable(
                name: "MucKinhNghiem");

            migrationBuilder.DropTable(
                name: "NhaTuyenDung");

            migrationBuilder.DropTable(
                name: "DiaDiem");

            migrationBuilder.DropTable(
                name: "TrinhDoHocVan");

            migrationBuilder.DropTable(
                name: "NganhNghe");

            migrationBuilder.DropTable(
                name: "QuyMoCongTy");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
