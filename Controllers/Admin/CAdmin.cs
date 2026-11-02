using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private const string StatusDraft = "Bản nháp";
        private const string StatusOpen = "Đang tuyển";
        private const string StatusRejected = "Từ chối";
        private readonly WorkConnectDbContext _context;

        public AdminController(WorkConnectDbContext context)
        {
            _context = context;
        }

        private const int PageSize = 10; // số dòng 1 trang
        private const int MaxSearchLength = 100;

        private static readonly string[] ValidTabs = { "approval", "users", "content", "applications" };

        public async Task<IActionResult> Index(
            string? tab, string? pq, int pp = 1, string? uq = null, int up = 1,
            string? cq = null, int cp = 1, string? aq = null, int ap = 1)
        {
            tab = ValidTabs.Contains(tab) ? tab! : "approval";
            pq = NormalizeSearch(pq);
            uq = NormalizeSearch(uq);
            cq = NormalizeSearch(cq);
            aq = NormalizeSearch(aq);

            var ajax = Request.IsAjaxRequest();

            var model = new AdminDashboardViewModel
            {
                ActiveTab = tab,

                // Số liệu tổng quan (không phụ thuộc bộ lọc)
                TotalUsers = await _context.TaiKhoans.CountAsync(),
                TotalJobs = await _context.TinTuyenDungs.CountAsync(),
                PendingApprovals = await _context.TinTuyenDungs.CountAsync(j => j.TrangThai == StatusDraft),
                TotalApplications = await _context.DonUngTuyens.CountAsync()
            };

            // Tải trang đầy đủ: dựng cả 4 tab. Request AJAX: chỉ dựng đúng tab đang xem cho nhẹ.
            if (!ajax || tab == "approval") model.PendingJobs = await BuildPendingJobsAsync(pq, pp);
            if (!ajax || tab == "users") model.Users = await BuildUsersAsync(uq, up);
            if (!ajax || tab == "content") model.Content = await BuildContentAsync(cq, cp);
            if (!ajax || tab == "applications") model.Applications = await BuildApplicationsAsync(aq, ap);

            if (ajax)
            {
                // AJAX (đổi tab / tìm kiếm / phân trang / làm mới sau thao tác): chỉ trả phần nội dung của tab
                Response.Headers.CacheControl = "no-store, max-age=0";
                ViewData["Tab"] = tab;
                return PartialView("_AdminTabBody", model);
            }

            return View(model);
        }

        // ===================== DỰNG DỮ LIỆU TỪNG TAB =====================

        // ===== Tab "Duyệt nội dung": tin ở trạng thái Bản nháp =====
        private async Task<PagedResult<JobPendingViewModel>> BuildPendingJobsAsync(string? pq, int pp)
        {
            var pendingQuery = _context.TinTuyenDungs.Where(j => j.TrangThai == StatusDraft);
            if (pq != null)
            {
                pendingQuery = pendingQuery.Where(j =>
                    j.TieuDe.Contains(pq) ||
                    (j.NhaTuyenDung != null && j.NhaTuyenDung.TenCongTy.Contains(pq)) ||
                    (j.NganhNghe != null && j.NganhNghe.TenNganhNghe.Contains(pq)));
            }
            var pendingSource = pendingQuery
                .OrderByDescending(j => j.NgayDang).ThenByDescending(j => j.MaTin)   // ThenBy: giữ thứ tự ổn định giữa các trang
                .Select(j => new
                {
                    j.MaTin, j.TieuDe,
                    Employer = j.NhaTuyenDung != null ? j.NhaTuyenDung.TenCongTy : null,
                    Category = j.NganhNghe != null ? j.NganhNghe.TenNganhNghe : null,
                    j.NgayDang, j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan,
                    j.ViTri, j.SoLuongTuyen, j.HanUngTuyen, j.MoTaCongViec, j.YeuCauCongViec,
                    Location = j.DiaDiem != null ? j.DiaDiem.TenDiaDiem : null,
                    EmployerAddress = j.NhaTuyenDung != null ? j.NhaTuyenDung.DiaChi : null
                });

            return await PagedResult<JobPendingViewModel>.CreateAsync(
                pendingSource,
                j => new JobPendingViewModel
                {
                    JobId = j.MaTin,
                    Title = j.TieuDe,
                    EmployerName = j.Employer ?? "N/A",
                    Category = j.Category ?? "Chưa phân loại",
                    PostedDate = j.NgayDang,
                    SalaryRange = FormatSalary(j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan),
                    Position = j.ViTri ?? string.Empty,
                    Location = j.Location ?? "Chưa cập nhật",
                    Quantity = j.SoLuongTuyen?.ToString("N0") ?? "—",
                    Deadline = j.HanUngTuyen,
                    Description = j.MoTaCongViec ?? string.Empty,
                    Requirements = j.YeuCauCongViec ?? string.Empty,
                    EmployerAddress = j.EmployerAddress ?? string.Empty
                },
                pp, PageSize, pq, "approval", "pq", "pp");
        }

        // ===== Tab "Quản lý người dùng" =====
        private async Task<PagedResult<UserAdminViewModel>> BuildUsersAsync(string? uq, int up)
        {
            var usersQuery = _context.TaiKhoans.AsQueryable();
            if (uq != null)
            {
                // Cho phép gõ "USR0012" hoặc "12" để tìm theo mã; gõ tên vai trò ("nhà tuyển dụng"...) để lọc theo vai trò
                var idText = uq.StartsWith("USR", StringComparison.OrdinalIgnoreCase) ? uq[3..] : uq;
                var userId = int.TryParse(idText, out var n) ? n : -1;

                var lower = uq.ToLowerInvariant();
                var roleIds = new List<int>();
                if ("admin".Contains(lower)) roleIds.Add(AppRoles.AdminId);
                if ("nhà tuyển dụng".Contains(lower)) roleIds.Add(AppRoles.NhaTuyenDungId);
                if ("ứng viên".Contains(lower)) roleIds.Add(AppRoles.UngVienId);

                usersQuery = usersQuery.Where(u =>
                    u.HoTen.Contains(uq) || u.Email.Contains(uq) ||
                    u.MaTaiKhoan == userId || roleIds.Contains(u.MaVaiTro));
            }
            var usersSource = usersQuery
                .OrderByDescending(u => u.NgayTao).ThenByDescending(u => u.MaTaiKhoan)
                .Select(u => new { u.MaTaiKhoan, u.HoTen, u.Email, u.MaVaiTro, u.NgayTao, u.BiKhoa });

            return await PagedResult<UserAdminViewModel>.CreateAsync(
                usersSource,
                u => new UserAdminViewModel
                {
                    RawId = u.MaTaiKhoan,
                    UserId = $"USR{u.MaTaiKhoan:D4}",
                    FullName = u.HoTen,
                    Email = u.Email,
                    Role = u.MaVaiTro == 1 ? "Admin" : (u.MaVaiTro == 2 ? "Nhà tuyển dụng" : "Ứng viên"),
                    CreatedDate = u.NgayTao,
                    IsActive = !u.BiKhoa
                },
                up, PageSize, uq, "users", "uq", "up");
        }

        // ===== Tab "Quản lý bài đăng": toàn bộ tin, mọi trạng thái =====
        private async Task<PagedResult<ContentAdminViewModel>> BuildContentAsync(string? cq, int cp)
        {
            var contentQuery = _context.TinTuyenDungs.AsQueryable();
            if (cq != null)
            {
                contentQuery = contentQuery.Where(j =>
                    j.TieuDe.Contains(cq) ||
                    (j.NhaTuyenDung != null && j.NhaTuyenDung.TenCongTy.Contains(cq)));
            }
            var contentSource = contentQuery
                .OrderByDescending(j => j.NgayDang).ThenByDescending(j => j.MaTin)
                .Select(j => new
                {
                    j.MaTin, j.TieuDe,
                    Employer = j.NhaTuyenDung != null ? j.NhaTuyenDung.TenCongTy : null,
                    j.NgayDang, j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan, j.SoLuongTuyen, j.TrangThai, j.BiAn
                });

            return await PagedResult<ContentAdminViewModel>.CreateAsync(
                contentSource,
                j => new ContentAdminViewModel
                {
                    JobId = j.MaTin,
                    Title = j.TieuDe,
                    EmployerName = j.Employer ?? "N/A",
                    Salary = FormatSalary(j.LuongToiThieu, j.LuongToiDa, j.LuongThoaThuan),
                    Quantity = j.SoLuongTuyen?.ToString("N0") ?? "—",
                    PostedDate = j.NgayDang,
                    Status = j.TrangThai,
                    IsHidden = j.BiAn
                },
                cp, PageSize, cq, "content", "cq", "cp");
        }

        // ===== Tab "Đơn ứng tuyển": ghép đơn + tin tuyển dụng + ứng viên + nhà tuyển dụng =====
        private async Task<PagedResult<ApplicationAdminViewModel>> BuildApplicationsAsync(string? aq, int ap)
        {
            var applicationsBase =
                from d in _context.DonUngTuyens
                join t in _context.TinTuyenDungs on d.MaTin equals t.MaTin
                join u in _context.TaiKhoans on d.MaUngVien equals u.MaTaiKhoan
                join e in _context.NhaTuyenDungs on t.MaNhaTuyenDung equals e.MaNhaTuyenDung into eg
                from e in eg.DefaultIfEmpty()
                select new
                {
                    d.MaDon,
                    d.HoTen,
                    d.Email,
                    d.SoDienThoai,
                    t.TieuDe,
                    TenCongTy = e != null ? e.TenCongTy : null,
                    d.NgayNop
                };
            if (aq != null)
            {
                // Cho phép gõ "#12" hoặc "12" để tìm theo mã đơn
                var appId = int.TryParse(aq.TrimStart('#'), out var n) ? n : -1;
                applicationsBase = applicationsBase.Where(a =>
                    a.MaDon == appId ||
                    a.HoTen.Contains(aq) || a.Email.Contains(aq) || a.SoDienThoai.Contains(aq) ||
                    a.TieuDe.Contains(aq) || (a.TenCongTy != null && a.TenCongTy.Contains(aq)));
            }
            var applicationsSource = applicationsBase
                .OrderByDescending(a => a.NgayNop).ThenByDescending(a => a.MaDon);

            return await PagedResult<ApplicationAdminViewModel>.CreateAsync(
                applicationsSource,
                a => new ApplicationAdminViewModel
                {
                    ApplicationId = a.MaDon,
                    CandidateName = string.IsNullOrWhiteSpace(a.HoTen) ? "N/A" : a.HoTen,
                    CandidateEmail = a.Email,
                    CandidatePhone = a.SoDienThoai,
                    JobTitle = a.TieuDe,
                    EmployerName = a.TenCongTy ?? "N/A",
                    AppliedDate = a.NgayNop
                },
                ap, PageSize, aq, "applications", "aq", "ap");
        }

        // ===================== CÁC THAO TÁC (AJAX) =====================
        // Gọi bằng AJAX -> trả JSON { success, message }; không phải AJAX (JS bị tắt) -> TempData + redirect như cũ.

        // Action xử lý Duyệt tin (gọi từ modal "Xem chi tiết")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveJob(int id, string? returnUrl = null)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);
            if (job == null)
                return Failed("Không tìm thấy tin tuyển dụng.", returnUrl, StatusCodes.Status404NotFound);

            // Chỉ xử lý tin còn đang chờ duyệt (tránh duyệt lại khi trang đã cũ / bấm 2 lần)
            if (job.TrangThai != StatusDraft)
                return Failed("Tin này đã được xử lý trước đó.", returnUrl, StatusCodes.Status409Conflict);

            job.TrangThai = StatusOpen;
            await _context.SaveChangesAsync();

            return Succeeded($"Đã duyệt tin \"{job.TieuDe}\".", returnUrl);
        }

        // Action xử lý Từ chối tin (gọi từ modal "Xem chi tiết")
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectJob(int id, string? returnUrl = null)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);
            if (job == null)
                return Failed("Không tìm thấy tin tuyển dụng.", returnUrl, StatusCodes.Status404NotFound);

            if (job.TrangThai != StatusDraft)
                return Failed("Tin này đã được xử lý trước đó.", returnUrl, StatusCodes.Status409Conflict);

            job.TrangThai = StatusRejected;
            await _context.SaveChangesAsync();

            return Succeeded($"Đã từ chối tin \"{job.TieuDe}\".", returnUrl);
        }

        // Action ẩn / hiện bài đăng (tab Quản lý bài đăng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJobVisibility(int id, string? returnUrl = null)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);
            if (job == null)
                return Failed("Không tìm thấy bài đăng.", returnUrl, StatusCodes.Status404NotFound);

            job.BiAn = !job.BiAn;
            await _context.SaveChangesAsync();

            return Succeeded(job.BiAn ? "Đã ẩn bài đăng." : "Đã hiện lại bài đăng.", returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJob(int id, string? returnUrl = null)
        {
            var job = await _context.TinTuyenDungs.FindAsync(id);

            if (job == null)
                return Failed("Không tìm thấy bài đăng cần xóa.", returnUrl, StatusCodes.Status404NotFound);

            try
            {
                _context.TinTuyenDungs.Remove(job);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Còn dữ liệu liên quan chặn việc xóa -> gợi ý ẩn bài đăng thay vì để lỗi 500
                _context.ChangeTracker.Clear();
                return Failed("Không thể xóa bài đăng vì còn dữ liệu liên quan (đơn ứng tuyển...). Hãy dùng chức năng Ẩn thay thế.",
                    returnUrl, StatusCodes.Status409Conflict);
            }

            return Succeeded("Xóa bài đăng thành công.", returnUrl);
        }

        // Action xử lý Khóa / Mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(int id, string? returnUrl = null)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null)
                return Failed("Không tìm thấy tài khoản.", returnUrl, StatusCodes.Status404NotFound);

            // Không cho khóa chính mình hoặc tài khoản Admin khác
            if (IsCurrentUser(user.MaTaiKhoan) || user.MaVaiTro == AppRoles.AdminId)
                return Failed("Không thể khóa tài khoản quản trị viên.", returnUrl, StatusCodes.Status403Forbidden);

            user.BiKhoa = !user.BiKhoa;
            await _context.SaveChangesAsync();

            return Succeeded(user.BiKhoa
                ? $"Đã khóa tài khoản {user.Email}."
                : $"Đã mở khóa tài khoản {user.Email}.", returnUrl);
        }

        // Action xử lý xóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int id, string? returnUrl = null)
        {
            var user = await _context.TaiKhoans.FindAsync(id);
            if (user == null)
                return Failed("Không tìm thấy tài khoản.", returnUrl, StatusCodes.Status404NotFound);

            // Không cho xóa chính mình hoặc tài khoản Admin khác
            if (IsCurrentUser(user.MaTaiKhoan) || user.MaVaiTro == AppRoles.AdminId)
                return Failed("Không thể xóa tài khoản quản trị viên.", returnUrl, StatusCodes.Status403Forbidden);

            try
            {
                // Xóa dữ liệu phụ thuộc (đơn ứng tuyển) trước để không vướng ràng buộc khóa ngoại
                var applications = _context.DonUngTuyens.Where(d => d.MaUngVien == user.MaTaiKhoan);
                _context.DonUngTuyens.RemoveRange(applications);

                _context.TaiKhoans.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Vẫn còn dữ liệu liên quan (vd: nhà tuyển dụng đang có tin đăng) -> gợi ý khóa thay vì xóa
                _context.ChangeTracker.Clear();
                return Failed("Không thể xóa vì tài khoản còn dữ liệu liên quan (tin đăng, hồ sơ...). Hãy dùng chức năng Khóa thay thế.",
                    returnUrl, StatusCodes.Status409Conflict);
            }

            return Succeeded("Xóa tài khoản thành công.", returnUrl);
        }

        // Kết quả thành công: AJAX -> JSON; thường -> TempData + quay lại trang đang xem
        private IActionResult Succeeded(string message, string? returnUrl)
        {
            if (Request.IsAjaxRequest())
                return Json(new { success = true, message });

            TempData["Success"] = message;
            return RedirectBack(returnUrl);
        }

        // Kết quả thất bại: AJAX -> JSON kèm mã lỗi HTTP; thường -> TempData + quay lại trang đang xem
        private IActionResult Failed(string message, string? returnUrl, int statusCode)
        {
            if (Request.IsAjaxRequest())
                return StatusCode(statusCode, new { success = false, message });

            TempData["Error"] = message;
            return RedirectBack(returnUrl);
        }

        // Quay lại đúng tab / trang / từ khóa đang xem (chỉ chấp nhận URL nội bộ để tránh open-redirect)
        private IActionResult RedirectBack(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));

        // Cắt khoảng trắng, giới hạn độ dài, chuẩn hóa Unicode (NFC) để gõ tiếng Việt có dấu luôn khớp
        private static string? NormalizeSearch(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim().Normalize();
            return s.Length > MaxSearchLength ? s[..MaxSearchLength] : s;
        }

        private bool IsCurrentUser(int maTaiKhoan) =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value == maTaiKhoan.ToString();

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
