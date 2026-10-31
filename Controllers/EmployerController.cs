
using System.Globalization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Data;
using WorkConnect.Models.Employer;
using WorkConnect.Models.Entities;

namespace WorkConnect.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

       
       // Trang chính của nhà tuyển dụng
        
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Index()
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound("Tài khoản chưa có hồ sơ nhà tuyển dụng.");

            var jobs = await _context.TinTuyenDungs
                .AsNoTracking()
                .Where(x => x.MaNhaTuyenDung == maNhaTuyenDung.Value)
                .OrderByDescending(x => x.NgayDang)
                .ToListAsync();

            var model = new WorkConnect.Models.Employer.EmployerDashboardViewModel
            {
                TotalJobs = jobs.Count,

                RecruitingJobs = jobs.Count(x =>
                    (x.TrangThai ?? "").Contains(
                        "đang tuyển", StringComparison.OrdinalIgnoreCase)),

                PendingJobs = jobs.Count(x =>
                    (x.TrangThai ?? "").Contains(
                        "chờ duyệt", StringComparison.OrdinalIgnoreCase)),

                ExpiredJobs = jobs.Count(x =>
                    (x.TrangThai ?? "").Contains(
                        "hết hạn", StringComparison.OrdinalIgnoreCase)),

                TotalApplications = await _context.DonUngTuyens
                    .CountAsync(x =>
                        x.TinTuyenDung != null &&
                        x.TinTuyenDung.MaNhaTuyenDung == maNhaTuyenDung.Value),

                RecentJobs = jobs.Take(5).ToList()
            };

            return View(model);
        }


        // Chi tiết hồ sơ nhà tuyển dụng
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Details(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var employer = await _context.NhaTuyenDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaNhaTuyenDung == id &&
                    x.MaTaiKhoan == maTaiKhoan.Value);

            if (employer == null)
                return NotFound();

            return View(employer);
        }

        // Danh sách tin của nhà tuyển dụng đang đăng nhập
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Jobs()
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound(
                    "Tài khoản chưa có hồ sơ nhà tuyển dụng.");

            var jobs = await _context.TinTuyenDungs
                .AsNoTracking()
                .Where(x =>
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value)
                .OrderByDescending(x => x.NgayDang)
                .ToListAsync();

            return View(jobs);
        }
        
        
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> JobDetails(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound();

            // Chỉ lấy tin thuộc nhà tuyển dụng đang đăng nhập.
            // Đồng thời lấy thông tin liên quan để hiển thị chi tiết.
            var job = await _context.TinTuyenDungs
                .AsNoTracking()
                .Include(x => x.NhaTuyenDung)
                .Include(x => x.NganhNghe)
                .Include(x => x.LoaiHinh)
                .Include(x => x.HinhThuc)
                .Include(x => x.DiaDiem)
                .Include(x => x.MucKinhNghiem)
                .Include(x => x.DonUngTuyens)
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound();

            // Cung cấp dữ liệu cho giao diện JobDetails.cshtml.
            ViewBag.TenCongTy =
                job.NhaTuyenDung?.TenCongTy ?? "Chưa cập nhật";

            ViewBag.MoTa =
                job.MoTaCongViec;

            ViewBag.YeuCau =
                string.IsNullOrWhiteSpace(job.YeuCauCongViec)
                    ? job.YeuCauUngVien
                    : job.YeuCauCongViec;

            ViewBag.QuyenLoi =
                job.QuyenLoi ?? "Chưa cập nhật quyền lợi.";

            ViewBag.ThongTinBoSung =
                job.YeuCauUngVien;

            ViewBag.SoLuong =
                job.SoLuongTuyen ?? 0;

            ViewBag.NganhNghe =
                job.NganhNghe?.TenNganhNghe ?? "Chưa cập nhật";

            ViewBag.DiaDiem =
                job.DiaDiem?.TenDiaDiem ?? "Chưa cập nhật";

            ViewBag.MucLuong = job.LuongThoaThuan
                ? "Thỏa thuận"
                : job.LuongToiThieu.HasValue || job.LuongToiDa.HasValue
                    ? $"{(job.LuongToiThieu.HasValue ? job.LuongToiThieu.Value.ToString("N0") : "0")} - {(job.LuongToiDa.HasValue ? job.LuongToiDa.Value.ToString("N0") : "Thỏa thuận")} VNĐ"
                    : "Chưa cập nhật";

            ViewBag.KinhNghiem =
                job.MucKinhNghiem?.TenMucKinhNghiem ?? "Không yêu cầu";

            ViewBag.HinhThuc =
                job.HinhThuc?.TenHinhThuc ?? "Chưa cập nhật";

            ViewBag.SoHoSo =
                job.DonUngTuyens.Count;

            return View(job);
        }


        
        // GET: Employer/Edit/13
        // Mở biểu mẫu sửa tin tuyển dụng
        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound();

            // Chỉ lấy tin thuộc nhà tuyển dụng đang đăng nhập
            var job = await _context.TinTuyenDungs
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound();

            var model = new CreateJobViewModel
            {
                TieuDe = job.TieuDe,
                ViTri = job.ViTri,
                MaNganhNghe = job.MaNganhNghe,
                MaLoaiHinh = job.MaLoaiHinh,
                MaHinhThuc = job.MaHinhThuc,
                MaDiaDiem = job.MaDiaDiem,
                MaMucKinhNghiem = job.MaMucKinhNghiem,
                LuongToiThieu = job.LuongToiThieu,
                LuongToiDa = job.LuongToiDa,
                LuongThoaThuan = job.LuongThoaThuan,
                SoLuongTuyen = job.SoLuongTuyen,
                MoTaCongViec = job.MoTaCongViec,
                YeuCauCongViec = job.YeuCauCongViec,
                YeuCauUngVien = job.YeuCauUngVien,
                QuyenLoi = job.QuyenLoi,
                HanUngTuyen = job.HanUngTuyen
            };

            await LoadJobCatalogs(model);

            ViewBag.MaTin = job.MaTin;
            ViewData["Title"] = "Sửa tin tuyển dụng";

            return View(model);
        }

        // POST: Employer/Edit/13
        // Lưu nội dung tin đã chỉnh sửa
        [Authorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CreateJobViewModel model)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound();

            // Kiểm tra quyền sở hữu lại khi lưu
            var job = await _context.TinTuyenDungs
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound();

            if (model.HanUngTuyen.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.HanUngTuyen),
                    "Hạn ứng tuyển phải từ hôm nay trở đi.");
            }

            if (!model.LuongThoaThuan &&
                model.LuongToiThieu.HasValue &&
                model.LuongToiDa.HasValue &&
                model.LuongToiThieu > model.LuongToiDa)
            {
                ModelState.AddModelError(
                    nameof(model.LuongToiDa),
                    "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu.");
            }

            if (!model.LuongThoaThuan &&
                !model.LuongToiThieu.HasValue &&
                !model.LuongToiDa.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.LuongToiThieu),
                    "Vui lòng nhập mức lương hoặc chọn lương thỏa thuận.");
            }

            if (!ModelState.IsValid)
            {
                await LoadJobCatalogs(model);
                ViewBag.MaTin = id;
                ViewData["Title"] = "Sửa tin tuyển dụng";
                return View(model);
            }

            job.TieuDe = model.TieuDe;
            job.ViTri = model.ViTri;
            job.MaNganhNghe = model.MaNganhNghe;
            job.MaLoaiHinh = model.MaLoaiHinh;
            job.MaHinhThuc = model.MaHinhThuc;
            job.MaDiaDiem = model.MaDiaDiem;
            job.MaMucKinhNghiem = model.MaMucKinhNghiem;
            job.LuongToiThieu = model.LuongToiThieu;
            job.LuongToiDa = model.LuongToiDa;
            job.LuongThoaThuan = model.LuongThoaThuan;
            job.SoLuongTuyen = model.SoLuongTuyen;
            job.MoTaCongViec = model.MoTaCongViec;
            job.YeuCauCongViec = model.YeuCauCongViec;
            job.YeuCauUngVien = model.YeuCauUngVien;
            job.QuyenLoi = model.QuyenLoi;
            job.HanUngTuyen = model.HanUngTuyen;

            // Không thay đổi MaNhaTuyenDung, MaTin,
            // NgayDang hoặc TrangThai tại đây.

            job.DuongDanThanThien =
                $"{CreateSlug(model.TieuDe)}-{job.MaTin}";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Cập nhật tin tuyển dụng thành công.";

            return RedirectToAction(nameof(Jobs));
        }

        // GET: Employer/Delete/13
        // Hiển thị trang xác nhận xóa tin
        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound();

            // Chỉ tìm tin thuộc nhà tuyển dụng đang đăng nhập
            var job = await _context.TinTuyenDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound();

            // Kiểm tra tin đã có đơn ứng tuyển chưa
            ViewBag.HasApplications =
                await _context.DonUngTuyens
                    .AnyAsync(x => x.MaTin == id);

            return View(job);
        }

        // POST: Employer/Delete/13
        // Thực hiện xóa sau khi người dùng xác nhận
        [Authorize(Roles = "Employer")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound();

            // Kiểm tra lại quyền sở hữu ngay trước khi xóa
            var job = await _context.TinTuyenDungs
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound();

            // Không xóa tin đã có đơn ứng tuyển
            var hasApplications =
                await _context.DonUngTuyens
                    .AnyAsync(x => x.MaTin == id);

            if (hasApplications)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa tin đã có đơn ứng tuyển.";

                return RedirectToAction(nameof(Jobs));
            }

            _context.TinTuyenDungs.Remove(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Xóa tin tuyển dụng thành công.";

            return RedirectToAction(nameof(Jobs));
        }


        // Nạp dữ liệu cho các danh sách lựa chọn khi đăng tin
        private async Task LoadJobCatalogs(CreateJobViewModel model)
        {
            model.NganhNghes = await _context.NganhNghes
                .AsNoTracking()
                .OrderBy(x => x.TenNganhNghe)
                .Select(x => new SelectListItem
                {
                    Value = x.MaNganhNghe.ToString(),
                    Text = x.TenNganhNghe
                })
                .ToListAsync();

            model.LoaiHinhs = await _context.LoaiHinhCongViecs
                .AsNoTracking()
                .OrderBy(x => x.TenLoaiHinh)
                .Select(x => new SelectListItem
                {
                    Value = x.MaLoaiHinh.ToString(),
                    Text = x.TenLoaiHinh
                })
                .ToListAsync();

            model.HinhThucs = await _context.HinhThucLamViecs
                .AsNoTracking()
                .OrderBy(x => x.TenHinhThuc)
                .Select(x => new SelectListItem
                {
                    Value = x.MaHinhThuc.ToString(),
                    Text = x.TenHinhThuc
                })
                .ToListAsync();

            model.DiaDiems = await _context.DiaDiems
                .AsNoTracking()
                .OrderBy(x => x.TenDiaDiem)
                .Select(x => new SelectListItem
                {
                    Value = x.MaDiaDiem.ToString(),
                    Text = x.TenDiaDiem
                })
                .ToListAsync();

            model.MucKinhNghiems = await _context.MucKinhNghiems
                .AsNoTracking()
                .OrderBy(x => x.TenMucKinhNghiem)
                .Select(x => new SelectListItem
                {
                    Value = x.MaMucKinhNghiem.ToString(),
                    Text = x.TenMucKinhNghiem
                })
                .ToListAsync();
        }

        // Mở trang đăng tin
        [HttpGet]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create()
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var employerExists = await _context.NhaTuyenDungs
                .AnyAsync(x => x.MaTaiKhoan == maTaiKhoan.Value);

            if (!employerExists)
                return NotFound(
                    "Tài khoản chưa có hồ sơ nhà tuyển dụng.");

            var model = new CreateJobViewModel
            {
                HanUngTuyen = DateTime.Today.AddDays(30)
            };

            await LoadJobCatalogs(model);

            return View(model);
        }

        // Lưu tin tuyển dụng
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create(
            CreateJobViewModel model)
        {
            // Kiểm tra hạn ứng tuyển
            if (model.HanUngTuyen.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.HanUngTuyen),
                    "Hạn ứng tuyển không được trước ngày hiện tại.");
            }

            // Kiểm tra khoảng lương
            if (model.LuongToiThieu.HasValue &&
                model.LuongToiDa.HasValue &&
                model.LuongToiDa < model.LuongToiThieu)
            {
                ModelState.AddModelError(
                    nameof(model.LuongToiDa),
                    "Lương tối đa phải lớn hơn hoặc bằng lương tối thiểu.");
            }

            // Nếu không thỏa thuận lương thì phải nhập ít nhất một mức lương
            if (!model.LuongThoaThuan &&
                !model.LuongToiThieu.HasValue &&
                !model.LuongToiDa.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.LuongThoaThuan),
                    "Vui lòng nhập mức lương hoặc chọn lương thỏa thuận.");
            }

            if (!ModelState.IsValid)
            {
                await LoadJobCatalogs(model);
                return View(model);
            }

            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            // Xác định hồ sơ nhà tuyển dụng từ tài khoản đăng nhập
            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản chưa có hồ sơ nhà tuyển dụng.");

                await LoadJobCatalogs(model);
                return View(model);
            }

            // Tạo đường dẫn thân thiện duy nhất
            string slug = CreateSlug(model.TieuDe);
            string uniqueSlug =
                $"{slug}-{DateTime.Now:yyyyMMddHHmmssfff}";

            var job = new TinTuyenDung
            {
                MaNhaTuyenDung = maNhaTuyenDung.Value,
                TieuDe = model.TieuDe.Trim(),
                DuongDanThanThien = uniqueSlug,
                ViTri = model.ViTri.Trim(),
                MaNganhNghe = model.MaNganhNghe,
                MaLoaiHinh = model.MaLoaiHinh,
                MaHinhThuc = model.MaHinhThuc,
                MaDiaDiem = model.MaDiaDiem,
                MaMucKinhNghiem = model.MaMucKinhNghiem,
                LuongToiThieu = model.LuongToiThieu,
                LuongToiDa = model.LuongToiDa,
                LuongThoaThuan = model.LuongThoaThuan,
                SoLuongTuyen = model.SoLuongTuyen,
                MoTaCongViec = model.MoTaCongViec.Trim(),
                YeuCauCongViec = model.YeuCauCongViec.Trim(),
                YeuCauUngVien = model.YeuCauUngVien.Trim(),
                QuyenLoi = model.QuyenLoi?.Trim(),
                NgayDang = DateTime.Now,
                HanUngTuyen = model.HanUngTuyen.Date,
                TrangThai = "Đang tuyển"
            };

            _context.TinTuyenDungs.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng tin thành công!";

            return RedirectToAction(nameof(Jobs));
        }

        // Lấy mã tài khoản từ cookie đăng nhập
        private int? GetMaTaiKhoan()
        {
            var maTaiKhoanText = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (int.TryParse(maTaiKhoanText, out int maTaiKhoan))
                return maTaiKhoan;

            return null;
        }

        // GET: Employer/Applications/5
        // Xem hồ sơ ứng tuyển của một tin thuộc nhà tuyển dụng hiện tại.
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Applications(int id)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound("Tài khoản chưa có hồ sơ nhà tuyển dụng.");

            // Kiểm tra tin tuyển dụng có thuộc nhà tuyển dụng đang đăng nhập không.
            var job = await _context.TinTuyenDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.MaTin == id &&
                    x.MaNhaTuyenDung == maNhaTuyenDung.Value);

            if (job == null)
                return NotFound("Không tìm thấy tin tuyển dụng.");

            // Chỉ lấy hồ sơ ứng tuyển thuộc tin đã xác minh ở trên.
            var applications = await _context.DonUngTuyens
                .AsNoTracking()
                .Where(x => x.MaTin == job.MaTin)
                .Include(x => x.UngVien)
                .Include(x => x.CV)
                .Include(x => x.TrangThai)
                .OrderByDescending(x => x.NgayNop)
                .ToListAsync();

            ViewBag.MaTin = job.MaTin;
            ViewBag.TieuDeTin = job.TieuDe;

            return View(applications);
        }
        
        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> SearchCandidates(
            string? keyword,
            string? chuyenNganh,
            string? kyNang,
            string? kinhNghiem)
        {
            var candidates = _context.UngViens
                .Include(x => x.DiaDiem)
                .Include(x => x.TrinhDo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                candidates = candidates.Where(x =>
                    x.HoTen.Contains(keyword) ||
                    x.Truong.Contains(keyword) ||
                    x.ChuyenNganh.Contains(keyword) ||
                    x.KyNang.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(chuyenNganh))
            {
                candidates = candidates.Where(x =>
                    x.ChuyenNganh == chuyenNganh);
            }

            if (!string.IsNullOrWhiteSpace(kyNang))
            {
                candidates = candidates.Where(x =>
                    x.KyNang.Contains(kyNang));
            }

            if (!string.IsNullOrWhiteSpace(kinhNghiem))
            {
                candidates = candidates.Where(x =>
                    x.KinhNghiem != null &&
                    x.KinhNghiem.Contains(kinhNghiem));
            }

            var model = new CandidateSearchViewModel
            {
                Keyword = keyword,
                ChuyenNganh = chuyenNganh,
                ChuyenNganhOptions = await _context.UngViens
                    .AsNoTracking()
                    .Where(x => !string.IsNullOrWhiteSpace(x.ChuyenNganh))
                    .Select(x => x.ChuyenNganh)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync(),
                KyNang = kyNang,
                KinhNghiem = kinhNghiem,
                Candidates = await candidates
                    .OrderBy(x => x.HoTen)
                    .ToListAsync()
            };

            return View(model);
        }
        
        [Authorize(Roles = "Employer")]
        [HttpGet]
        public async Task<IActionResult> CandidateDetails(int id)
        {
            var candidate = await _context.UngViens
                .AsNoTracking()
                .Include(x => x.DiaDiem)
                .Include(x => x.TrinhDo)
                .FirstOrDefaultAsync(x => x.MaUngVien == id);

            if (candidate == null)
            {
                return NotFound("Không tìm thấy hồ sơ ứng viên.");
            }

            return View(candidate);
        }
        // POST: Employer/ReviewApplication
        // Nhà tuyển dụng xét duyệt hồ sơ ứng tuyển
        [Authorize(Roles = "Employer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewApplication(
            int maDon,
            int maTrangThai,
            string? ghiChuNoiBo,
            string? phanHoiUngVien,
            DateTime? thoiGianPhongVan,
            string? diaDiemPhongVan)
        {
            var maTaiKhoan = GetMaTaiKhoan();

            if (maTaiKhoan == null)
                return Challenge();

            var maNhaTuyenDung = await GetMaNhaTuyenDung(
                maTaiKhoan.Value);

            if (maNhaTuyenDung == null)
                return NotFound(
                    "Tài khoản chưa có hồ sơ nhà tuyển dụng.");

            // Chỉ cho phép trạng thái từ 2 đến 5.
            // Trạng thái 1 = Đã gửi là trạng thái ban đầu.
            if (maTrangThai < 2 || maTrangThai > 5)
            {
                return BadRequest("Trạng thái xét duyệt không hợp lệ.");
            }

            // Kiểm tra hồ sơ có thuộc tin của nhà tuyển dụng hiện tại không.
            var don = await _context.DonUngTuyens
                .Include(x => x.TinTuyenDung)
                .FirstOrDefaultAsync(x =>
                    x.MaDon == maDon &&
                    x.TinTuyenDung != null &&
                    x.TinTuyenDung.MaNhaTuyenDung
                        == maNhaTuyenDung.Value);

            if (don == null)
            {
                return NotFound(
                    "Không tìm thấy hồ sơ hoặc bạn không có quyền xét duyệt.");
            }

            // Nếu chọn Mời phỏng vấn thì bắt buộc có thời gian và địa điểm.
            if (maTrangThai == 3)
            {
                if (!thoiGianPhongVan.HasValue ||
                    string.IsNullOrWhiteSpace(diaDiemPhongVan))
                {
                    TempData["Error"] =
                        "Vui lòng nhập thời gian và địa điểm phỏng vấn.";

                    return RedirectToAction(
                        "Applications",
                        new { id = don.MaTin });
                }

                if (thoiGianPhongVan.Value <= DateTime.Now)
                {
                    TempData["Error"] =
                        "Thời gian phỏng vấn phải ở trong tương lai.";

                    return RedirectToAction(
                        "Applications",
                        new { id = don.MaTin });
                }
            }

            // Cập nhật trạng thái hiện tại của hồ sơ.
            don.MaTrangThai = maTrangThai;

            // Tạo bản ghi lịch sử xử lý.
            var lichSu = new LichSuXuLyDon
            {
                MaDon = don.MaDon,
                MaTrangThai = maTrangThai,
                ThoiGianPhongVan =
                    maTrangThai == 3 ? thoiGianPhongVan : null,
                DiaDiemPhongVan =
                    maTrangThai == 3 ? diaDiemPhongVan : null,
                GhiChuNoiBo = ghiChuNoiBo,
                PhanHoiUngVien = phanHoiUngVien,
                NgayCapNhat = DateTime.Now
            };

            _context.LichSuXuLyDons.Add(lichSu);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật trạng thái hồ sơ ứng tuyển.";

            return RedirectToAction(
                "Applications",
                new { id = don.MaTin });
        }



        // Tìm mã hồ sơ nhà tuyển dụng theo tài khoản
        private async Task<int?> GetMaNhaTuyenDung(int maTaiKhoan)
        {
            return await _context.NhaTuyenDungs
                .AsNoTracking()
                .Where(x => x.MaTaiKhoan == maTaiKhoan)
                .Select(x => (int?)x.MaNhaTuyenDung)
                .FirstOrDefaultAsync();
        }


        // Tạo slug từ tiêu đề
        private static string CreateSlug(string text)
        {
            string normalized = text.Normalize(
                NormalizationForm.FormD);

            var result = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) ==
                    UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    result.Append(char.ToLowerInvariant(c));
                }
                else if (result.Length > 0 && result[^1] != '-')
                {
                    result.Append('-');
                }
            }

            return result.ToString().Trim('-');
        }
    }
}
