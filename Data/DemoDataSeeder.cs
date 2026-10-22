using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Models.Entities;

namespace WorkConnect.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<TaiKhoan> hasher)
    {
        if (await db.TinTuyenDungs.AnyAsync()) return;

        TaiKhoan NewUser(int role, string email, string name, string phone)
        {
            var u = new TaiKhoan { MaVaiTro = role, Email = email, HoTen = name, SoDienThoai = phone };
            u.MatKhauMaHoa = hasher.HashPassword(u, "Demo@1234");
            return u;
        }

        // Tài khoản ứng viên để demo: ungvien@demo.vn / Demo@1234
        if (!await db.TaiKhoans.AnyAsync(x => x.Email == "ungvien@demo.vn"))
            db.TaiKhoans.Add(NewUser(3, "ungvien@demo.vn", "Nguyễn Văn Demo", "0912345678"));

        var companies = new (string Name, string Email, string Phone, string Addr, string Web, int Nganh, int QuyMo, string Intro)[]
        {
            ("Công ty TNHH Công nghệ Sao Việt", "hr@saoviet.vn", "0241234567", "Cầu Giấy, Hà Nội", "https://saoviet.vn", 1, 3, "Công ty phát triển phần mềm và giải pháp số cho doanh nghiệp."),
            ("Công ty CP Truyền thông Hoàng Mai", "hr@hoangmai.vn", "0241234568", "Hai Bà Trưng, Hà Nội", "https://hoangmai.vn", 3, 2, "Agency truyền thông và quảng cáo số."),
            ("Công ty TNHH Kế toán An Phát", "hr@anphat.vn", "0241234569", "Đống Đa, Hà Nội", "https://anphat.vn", 2, 1, "Dịch vụ kế toán, thuế cho doanh nghiệp vừa và nhỏ."),
            ("Công ty CP Thương mại Đại Dương", "hr@daiduong.vn", "0281234567", "Quận 1, TP. Hồ Chí Minh", "https://daiduong.vn", 4, 2, "Phân phối hàng tiêu dùng trên toàn quốc."),
        };

        var emps = new List<NhaTuyenDung>();
        foreach (var c in companies)
        {
            var e = new NhaTuyenDung
            {
                TaiKhoan = NewUser(2, c.Email, c.Name, "0987654321"),
                TenCongTy = c.Name, EmailLienHe = c.Email, SoDienThoaiLienHe = c.Phone, DiaChi = c.Addr,
                Website = c.Web, MaNganhNghe = c.Nganh, MaQuyMo = c.QuyMo, GioiThieu = c.Intro
            };
            db.NhaTuyenDungs.Add(e);
            emps.Add(e);
        }

        // (Emp, Title, Slug, Pos, Nganh, Loai, Hinh, DiaDiem, KinhNghiem, Min, Max, SoLuong, NgayTruoc, ConLaiNgay)
        var jobs = new (int Emp, string Title, string Slug, string Pos, int Nganh, int Loai, int Hinh, int Dd, int Kn, decimal? Min, decimal? Max, int Sl, int Ago, int Left)[]
        {
            (0, "Thực tập sinh lập trình Web ASP.NET Core", "thuc-tap-sinh-lap-trinh-web-asp-net-core", "Thực tập sinh Web", 1, 1, 1, 1, 1, 3_000_000m, 5_000_000m, 5, 1, 30),
            (0, "Lập trình viên Front-end ReactJS (Fresher)", "lap-trinh-vien-front-end-reactjs-fresher", "Front-end Developer", 1, 4, 3, 1, 2, 10_000_000m, 15_000_000m, 3, 2, 25),
            (0, "Nhân viên kiểm thử phần mềm (Tester)", "nhan-vien-kiem-thu-phan-mem-tester", "Tester", 1, 3, 1, 2, 3, 12_000_000m, 18_000_000m, 2, 3, 20),
            (0, "Lập trình viên Backend .NET", "lap-trinh-vien-backend-net", "Backend Developer", 1, 3, 3, 3, 4, 20_000_000m, 30_000_000m, 2, 4, 15),
            (1, "Thực tập sinh Digital Marketing", "thuc-tap-sinh-digital-marketing", "Thực tập sinh Marketing", 3, 1, 1, 1, 1, 2_000_000m, 4_000_000m, 4, 1, 12),
            (1, "Content Marketing (Bán thời gian)", "content-marketing-ban-thoi-gian", "Content Marketing", 3, 2, 2, 2, 2, 5_000_000m, 8_000_000m, 2, 5, 28),
            (1, "Chuyên viên SEO", "chuyen-vien-seo", "SEO Specialist", 3, 3, 3, 1, 3, 10_000_000m, 14_000_000m, 1, 6, 5),
            (2, "Thực tập sinh Kế toán tổng hợp", "thuc-tap-sinh-ke-toan-tong-hop", "Thực tập sinh Kế toán", 2, 1, 1, 1, 1, null, null, 3, 2, 10),
            (2, "Kế toán thuế", "ke-toan-thue", "Kế toán thuế", 2, 3, 1, 4, 3, 9_000_000m, 13_000_000m, 1, 7, 22),
            (3, "Nhân viên kinh doanh (Mới tốt nghiệp)", "nhan-vien-kinh-doanh-moi-tot-nghiep", "Nhân viên kinh doanh", 4, 4, 1, 2, 1, 8_000_000m, 20_000_000m, 10, 3, 18),
            (3, "Nhân viên chăm sóc khách hàng online", "nhan-vien-cham-soc-khach-hang-online", "CSKH Online", 4, 2, 2, 1, 1, 6_000_000m, 9_000_000m, 5, 8, 9),
            (3, "Trợ lý kinh doanh", "tro-ly-kinh-doanh", "Trợ lý kinh doanh", 4, 3, 3, 5, 2, 7_000_000m, 10_000_000m, 1, 9, 3),
        };

        foreach (var j in jobs)
        {
            db.TinTuyenDungs.Add(new TinTuyenDung
            {
                NhaTuyenDung = emps[j.Emp], TieuDe = j.Title, DuongDanThanThien = j.Slug, ViTri = j.Pos,
                MaNganhNghe = j.Nganh, MaLoaiHinh = j.Loai, MaHinhThuc = j.Hinh, MaDiaDiem = j.Dd, MaMucKinhNghiem = j.Kn,
                LuongToiThieu = j.Min, LuongToiDa = j.Max, LuongThoaThuan = j.Min == null && j.Max == null, SoLuongTuyen = j.Sl,
                MoTaCongViec = $"- Thực hiện các công việc của vị trí {j.Pos}.\n- Phối hợp với các thành viên trong nhóm.\n- Được hướng dẫn và đào tạo khi làm việc.",
                YeuCauCongViec = "- Làm việc theo giờ hành chính.\n- Báo cáo tiến độ định kỳ cho quản lý trực tiếp.",
                YeuCauUngVien = "- Sinh viên năm cuối hoặc mới tốt nghiệp chuyên ngành phù hợp.\n- Chủ động, ham học hỏi.\n- Kỹ năng giao tiếp và làm việc nhóm tốt.",
                QuyenLoi = "- Môi trường trẻ trung, năng động.\n- Lộ trình phát triển rõ ràng.\n- Thưởng lễ, tết.",
                NgayDang = DateTime.Now.AddDays(-j.Ago), HanUngTuyen = DateTime.Today.AddDays(j.Left), TrangThai = "Đang tuyển"
            });
        }
        await db.SaveChangesAsync();
    }
}