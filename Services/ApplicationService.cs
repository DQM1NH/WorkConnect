using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;
using WorkConnect.Models.Entities;

namespace WorkConnect.Services;

public interface IApplicationService
{
    Task<ApplyPrepareResult> PrepareAsync(int taiKhoanId, string slug);
    Task<ApplySubmitResult> SubmitAsync(int taiKhoanId, string slug, ApplyForm form, IFormFile? newCv);
    Task<ApplicationsViewModel> GetMineAsync(int taiKhoanId, int? status);
}

public class ApplicationService : IApplicationService
{
    private readonly WorkConnectDbContext _db;
    private readonly ICandidateService _candidates;

    public ApplicationService(WorkConnectDbContext db, ICandidateService candidates)
    { _db = db; _candidates = candidates; }

    // Tin "Bản nháp" không công khai -> coi như không tồn tại
    private Task<ApplyViewModel?> FindJobAsync(string slug)
        => _db.TinTuyenDungs.AsNoTracking()
            .Where(t => t.DuongDanThanThien == slug && t.TrangThai != "Bản nháp")
            .Select(t => new ApplyViewModel
            {
                MaTin = t.MaTin, Slug = t.DuongDanThanThien, TieuDe = t.TieuDe,
                TenCongTy = t.NhaTuyenDung!.TenCongTy, HanUngTuyen = t.HanUngTuyen, TrangThaiTin = t.TrangThai
            }).FirstOrDefaultAsync();

    // ---------- UC07 bước 1-3: kiểm tra điều kiện + dựng form ----------
    public async Task<ApplyPrepareResult> PrepareAsync(int taiKhoanId, string slug)
    {
        var vm = await FindJobAsync(slug);
        if (vm == null) return new ApplyPrepareResult(ApplyCheck.JobNotFound);
        if (!vm.ConNhanDon) return new ApplyPrepareResult(ApplyCheck.JobClosed);

        var uv = await _db.UngViens.AsNoTracking().Where(u => u.MaTaiKhoan == taiKhoanId)
            .Select(u => new { u.MaUngVien, u.HoTen, u.EmailLienHe, u.SoDienThoaiLienHe }).FirstOrDefaultAsync();
        if (uv == null) return new ApplyPrepareResult(ApplyCheck.NoProfile);

        if (await _db.DonUngTuyens.AnyAsync(d => d.MaTin == vm.MaTin && d.MaUngVien == uv.MaUngVien))
            return new ApplyPrepareResult(ApplyCheck.AlreadyApplied);

        vm.Cvs = await _candidates.GetCvsAsync(taiKhoanId);
        vm.Form = new ApplyForm
        {
            HoTen = uv.HoTen, Email = uv.EmailLienHe, SoDienThoai = uv.SoDienThoaiLienHe,
            MaCV = vm.Cvs.FirstOrDefault()?.MaCV
        };
        return new ApplyPrepareResult(ApplyCheck.Ready, vm);
    }

    // ---------- UC07 bước 4-6: tạo đơn ----------
    public async Task<ApplySubmitResult> SubmitAsync(int taiKhoanId, string slug, ApplyForm form, IFormFile? newCv)
    {
        var job = await FindJobAsync(slug);
        if (job == null) return new ApplySubmitResult(ApplyCheck.JobNotFound);
        if (!job.ConNhanDon) return new ApplySubmitResult(ApplyCheck.JobClosed);      // R06: hết hạn

        var maUngVien = await _db.UngViens.AsNoTracking().Where(u => u.MaTaiKhoan == taiKhoanId)
            .Select(u => (int?)u.MaUngVien).FirstOrDefaultAsync();
        if (maUngVien == null) return new ApplySubmitResult(ApplyCheck.NoProfile);

        if (await _db.DonUngTuyens.AnyAsync(d => d.MaTin == job.MaTin && d.MaUngVien == maUngVien))
            return new ApplySubmitResult(ApplyCheck.AlreadyApplied);                  // R06: chặn nộp trùng

        int maCv;
        if (newCv is { Length: > 0 })
        {
            var add = await _candidates.AddCvAsync(taiKhoanId, newCv);                // kiểm tra loại/dung lượng tệp (R32)
            if (!add.Ok) return new ApplySubmitResult(ApplyCheck.Invalid, add.Error);
            maCv = add.Data!.MaCV;
        }
        else
        {
            if (form.MaCV is not int chosen)
                return new ApplySubmitResult(ApplyCheck.Invalid, "Vui lòng chọn một CV hoặc tải CV mới.");

            // Ownership check: CV phải thuộc về chính ứng viên này (chống IDOR)
            if (!await _db.CVUngViens.AnyAsync(c => c.MaCV == chosen && c.MaUngVien == maUngVien))
                return new ApplySubmitResult(ApplyCheck.Invalid, "CV được chọn không hợp lệ.");
            maCv = chosen;
        }

        var now = DateTime.Now;
        var don = new DonUngTuyen
        {
            MaTin = job.MaTin,                 // lấy từ slug trên server, không nhận từ form
            MaUngVien = maUngVien.Value,       // lấy từ tài khoản đăng nhập
            MaCV = maCv,
            HoTen = form.HoTen.Trim(),
            Email = form.Email.Trim(),
            SoDienThoai = form.SoDienThoai.Trim(),
            ThuGioiThieu = string.IsNullOrWhiteSpace(form.ThuGioiThieu) ? null : form.ThuGioiThieu.Trim(),
            NgayNop = now,
            MaTrangThai = 1                    // "Đã gửi"
        };
        don.LichSuXuLys.Add(new LichSuXuLyDon { MaTrangThai = 1, NgayCapNhat = now });   // dòng đầu của lịch sử
        _db.DonUngTuyens.Add(don);

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            // Hai request cùng lúc -> chỉ mục UNIQUE(MaTin, MaUngVien) chặn bản thứ hai
            _db.ChangeTracker.Clear();
            if (await _db.DonUngTuyens.AnyAsync(d => d.MaTin == job.MaTin && d.MaUngVien == maUngVien))
                return new ApplySubmitResult(ApplyCheck.AlreadyApplied);
            throw;
        }
        return new ApplySubmitResult(ApplyCheck.Ready, null, don.MaDon);
    }

    // ---------- UC08: danh sách đơn của tôi ----------
    public async Task<ApplicationsViewModel> GetMineAsync(int taiKhoanId, int? status)
    {
        var q = _db.DonUngTuyens.AsNoTracking().Where(d => d.UngVien!.MaTaiKhoan == taiKhoanId);
        if (status is int s) q = q.Where(d => d.MaTrangThai == s);

        // Chỉ chọn các cột ứng viên được phép xem: KHÔNG lấy GhiChuNoiBo
        var items = await q.OrderByDescending(d => d.NgayNop).Select(d => new ApplicationItem
        {
            MaDon = d.MaDon,
            TieuDe = d.TinTuyenDung!.TieuDe,
            Slug = d.TinTuyenDung!.DuongDanThanThien,
            TenCongTy = d.TinTuyenDung!.NhaTuyenDung!.TenCongTy,
            TenCV = d.CV!.TenTep,
            NgayNop = d.NgayNop,
            MaTrangThai = d.MaTrangThai,
            TenTrangThai = d.TrangThai!.TenTrangThai,
            LichSu = d.LichSuXuLys.OrderByDescending(h => h.NgayCapNhat).ThenByDescending(h => h.MaLichSu)
                .Select(h => new ApplicationHistoryItem
                {
                    MaTrangThai = h.MaTrangThai,
                    TenTrangThai = h.TrangThai!.TenTrangThai,
                    ThoiGianPhongVan = h.ThoiGianPhongVan,
                    DiaDiemPhongVan = h.DiaDiemPhongVan,
                    PhanHoiUngVien = h.PhanHoiUngVien,
                    NgayCapNhat = h.NgayCapNhat
                }).ToList()
        }).ToListAsync();

        return new ApplicationsViewModel
        {
            Items = items,
            Status = status,
            Statuses = await _db.TrangThaiDons.AsNoTracking().OrderBy(x => x.MaTrangThai)
                .Select(x => new OptionItem(x.MaTrangThai, x.TenTrangThai)).ToListAsync()
        };
    }
}