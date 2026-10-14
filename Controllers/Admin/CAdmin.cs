using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;

namespace WorkConnect.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        // Trạng thái tin tuyển dụng (khớp CHECK constraint CK_Tin_TrangThai trong CSDL)
        private const string StatusDraft = "Bản nháp";
        private const string StatusOpen = "Đang tuyển";
        private readonly WorkConnectDbContext _context;

        public AdminController(WorkConnectDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pendingRaw = await _context.TinTuyenDungs.Where(j => j.TrangThai == StatusDraft).OrderByDescending(j => j.NgayDang).Select(j => new
            {
                j.MaTin, j.TieuDe,
                Employer = j.NhaTuyenDung != null ? j.NhaTuyenDung.TenCongTy : null,
                Category = j.NganhNghe != null ? j.NganhNghe.TenNganhNghe : null,
                j.NgayDang, j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan
            }).ToListAsync();

            var contentRaw = await _context.TinTuyenDungs.OrderByDescending(j => j.NgayDang).Select(j => new
            {
                j.MaTin, j.TieuDe,
                Employer = j.NhaTuyenDung != null ? j.NhaTuyenDung.TenCongTy : null,
                j.NgayDang, j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan, j.SoLuongTuyen, j.TrangThai
            }).ToListAsync();

            var model = new AdminDashboardViewModel
            {
                // Lấy số liệu từ CSDL
                TotalUsers = await _context.TaiKhoans.CountAsync(),
                TotalJobs = await _context.TinTuyenDungs.CountAsync(),
                PendingApprovals = await _context.TinTuyenDungs.CountAsync(j => j.TrangThai == "Bản nháp"),
                TotalApplications = await _context.DonUngTuyens.CountAsync(),

                PendingJobs = pendingRaw.Select(j => new JobPendingViewModel
                {
                    JobId = j.MaTin,
                    Title = j.TieuDe,
                    EmployerName = j.Employer ?? "N/A",
                    Category = j.Category ?? "Chưa phân loại",
                    PostedDate = j.NgayDang,
                    SalaryRange = FormatSalary(j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan)
                }).ToList(),

                // Tab "Quản lý bài đăng": toàn bộ tin, mọi trạng thái
                Content = contentRaw.Select(j => new ContentAdminViewModel
                {
                    JobId = j.MaTin,
                    Title = j.TieuDe,
                    EmployerName = j.Employer ?? "N/A",
                    Salary = FormatSalary(j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan),
                    Quantity = j.SoLuongTuyen?.ToString("N0") ?? "—",
                    PostedDate = j.NgayDang,
                    Status = j.TrangThai
                }).ToList(),

                // Danh sách tài khoản người dùng
                Users = await _context.TaiKhoans.OrderByDescending(u => u.NgayTao).Take(20).Select(u => new UserAdminViewModel
                    {
                        UserId = $"USR{u.MaTaiKhoan:D4}",
                        FullName = u.HoTen,
                        Email = u.Email,
                        Role = u.MaVaiTro == 1 ? "Admin" : (u.MaVaiTro == 2 ? "Nhà tuyển dụng" : "Ứng viên"),
                        CreatedDate = u.NgayTao,
                        IsActive = !u.BiKhoa
                    }).ToListAsync()
            };

            return View(model);
        }

        // Action xử lý Duyệt tin
        [HttpPost]
        public async Task<IActionResult> ApproveJob(int id)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);
            if (job != null)
            {
                job.TrangThai = "Đang tuyển";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Action ẩn / hiện bài đăng (tab Quản lý bài đăng)
        [HttpPost]
        public async Task<IActionResult> ToggleJobVisibility(int id)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);
            if (job != null)
            {
                job.BiAn = !job.BiAn;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Action xử lý Khóa / Mở khóa tài khoản
        [HttpPost]
        public async Task<IActionResult> ToggleUserLock(int id)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user != null)
            {
                user.BiKhoa = !user.BiKhoa;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Định dạng mức lương hiển thị (đơn vị: triệu đồng); xử lý đủ các trường hợp null
        private static string FormatSalary(decimal? min, decimal? max, bool negotiable)
        {
            if (negotiable) return "Thỏa thuận";

            static string M(decimal v) => (v / 1_000_000m).ToString("0.##");

            if (min.HasValue && max.HasValue) return $"{M(min.Value)} - {M(max.Value)} triệu";
            if (min.HasValue) return $"Từ {M(min.Value)} triệu";
            if (max.HasValue) return $"Đến {M(max.Value)} triệu";
            return "Chưa cập nhật";
        }
    }
}