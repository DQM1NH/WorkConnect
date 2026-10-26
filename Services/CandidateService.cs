using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;
using WorkConnect.Models.Entities;

namespace WorkConnect.Services;

public interface ICandidateService
{
    Task<ProfileViewModel> GetProfileAsync(int taiKhoanId);
    Task<ServiceResult> SaveProfileAsync(int taiKhoanId, ProfileForm form, IFormFile? avatar);
    Task<StoredFile?> GetAvatarAsync(int taiKhoanId);

    Task<List<CvItem>> GetCvsAsync(int taiKhoanId);
    Task<bool> HasProfileAsync(int taiKhoanId);
    Task<ServiceResult<CvItem>> AddCvAsync(int taiKhoanId, IFormFile file);
    Task<ServiceResult> DeleteCvAsync(int taiKhoanId, int maCv);
    Task<StoredFile?> GetCvFileAsync(int taiKhoanId, int maCv);
}

public class CandidateService : ICandidateService
{
    private const int MaxCvPerCandidate = 10;

    private readonly WorkConnectDbContext _db;
    private readonly IWebHostEnvironment _env;

    public CandidateService(WorkConnectDbContext db, IWebHostEnvironment env)
    { _db = db; _env = env; }

    // File nằm NGOÀI wwwroot -> không truy cập được bằng URL trực tiếp, chỉ tải qua action có kiểm tra quyền.
    private string CvDir => Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "cv");
    private string AvatarDir => Path.Combine(_env.ContentRootPath, "App_Data", "uploads", "avatar");

    // ================= UC03 - Hồ sơ =================
    public async Task<ProfileViewModel> GetProfileAsync(int taiKhoanId)
    {
        var vm = new ProfileViewModel { Options = await GetOptionsAsync() };

        var uv = await _db.UngViens.AsNoTracking().FirstOrDefaultAsync(u => u.MaTaiKhoan == taiKhoanId);
        if (uv == null)
        {
            // Chưa có hồ sơ: lấy sẵn họ tên / email / SĐT từ tài khoản (BM02)
            var tk = await _db.TaiKhoans.AsNoTracking().Where(t => t.MaTaiKhoan == taiKhoanId)
                .Select(t => new { t.HoTen, t.Email, t.SoDienThoai }).FirstOrDefaultAsync();
            vm.Form = new ProfileForm
            {
                HoTen = tk?.HoTen ?? string.Empty,
                EmailLienHe = tk?.Email ?? string.Empty,
                SoDienThoaiLienHe = tk?.SoDienThoai ?? string.Empty
            };
            return vm;
        }

        vm.DaCoHoSo = true;
        vm.CoAnhDaiDien = !string.IsNullOrEmpty(uv.AnhDaiDien);
        vm.Form = new ProfileForm
        {
            HoTen = uv.HoTen, NgaySinh = uv.NgaySinh, GioiTinh = uv.GioiTinh,
            EmailLienHe = uv.EmailLienHe, SoDienThoaiLienHe = uv.SoDienThoaiLienHe,
            MaDiaDiem = uv.MaDiaDiem, Truong = uv.Truong, ChuyenNganh = uv.ChuyenNganh,
            MaTrinhDo = uv.MaTrinhDo, NamTotNghiep = uv.NamTotNghiep, KyNang = uv.KyNang,
            KinhNghiem = uv.KinhNghiem, ChungChi = uv.ChungChi, MucTieuNgheNghiep = uv.MucTieuNgheNghiep
        };
        return vm;
    }

    private async Task<ProfileOptions> GetOptionsAsync() => new()
    {
        DiaDiems = await _db.DiaDiems.AsNoTracking().OrderBy(x => x.MaDiaDiem)
            .Select(x => new OptionItem(x.MaDiaDiem, x.TenDiaDiem)).ToListAsync(),
        TrinhDos = await _db.TrinhDoHocVans.AsNoTracking().OrderBy(x => x.MaTrinhDo)
            .Select(x => new OptionItem(x.MaTrinhDo, x.TenTrinhDo)).ToListAsync()
    };

    public async Task<ServiceResult> SaveProfileAsync(int taiKhoanId, ProfileForm f, IFormFile? avatar)
    {
        // Không tin giá trị chọn từ form: kiểm tra mã danh mục có thật
        if (!await _db.TrinhDoHocVans.AnyAsync(x => x.MaTrinhDo == f.MaTrinhDo))
            return new ServiceResult(false, "Trình độ học vấn không hợp lệ.");
        if (f.MaDiaDiem is int dd && !await _db.DiaDiems.AnyAsync(x => x.MaDiaDiem == dd))
            return new ServiceResult(false, "Tỉnh/Thành phố không hợp lệ.");

        var skills = NormalizeSkills(f.KyNang);
        if (skills.Length == 0) return new ServiceResult(false, "Vui lòng nhập ít nhất một kỹ năng.");

        string? newAvatar = null;
        if (avatar is { Length: > 0 })
        {
            var err = UploadRules.CheckAvatar(avatar, out var ext);
            if (err != null) return new ServiceResult(false, err);
            newAvatar = await SaveToDiskAsync(avatar, AvatarDir, ext);
        }

        var uv = await _db.UngViens.FirstOrDefaultAsync(u => u.MaTaiKhoan == taiKhoanId);
        var oldAvatar = uv?.AnhDaiDien;
        if (uv == null)
        {
            uv = new UngVien { MaTaiKhoan = taiKhoanId };
            _db.UngViens.Add(uv);
        }

        uv.HoTen = f.HoTen.Trim();
        uv.NgaySinh = f.NgaySinh!.Value.Date;
        uv.GioiTinh = N(f.GioiTinh);
        uv.EmailLienHe = f.EmailLienHe.Trim();
        uv.SoDienThoaiLienHe = f.SoDienThoaiLienHe.Trim();
        uv.MaDiaDiem = f.MaDiaDiem;
        uv.Truong = f.Truong.Trim();
        uv.ChuyenNganh = f.ChuyenNganh.Trim();
        uv.MaTrinhDo = f.MaTrinhDo!.Value;
        uv.NamTotNghiep = f.NamTotNghiep;
        uv.KyNang = skills;
        uv.KinhNghiem = N(f.KinhNghiem);
        uv.ChungChi = N(f.ChungChi);
        uv.MucTieuNgheNghiep = N(f.MucTieuNgheNghiep);
        if (newAvatar != null) uv.AnhDaiDien = newAvatar;

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            if (newAvatar != null) DeleteQuiet(AvatarDir, newAvatar);
            return new ServiceResult(false, "Không thể lưu hồ sơ. Vui lòng kiểm tra lại dữ liệu.");
        }

        if (newAvatar != null && !string.IsNullOrEmpty(oldAvatar)) DeleteQuiet(AvatarDir, oldAvatar);
        return new ServiceResult(true);
    }

    public async Task<StoredFile?> GetAvatarAsync(int taiKhoanId)
    {
        var name = await _db.UngViens.AsNoTracking().Where(u => u.MaTaiKhoan == taiKhoanId)
            .Select(u => u.AnhDaiDien).FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(name)) return null;

        var path = Path.Combine(AvatarDir, Path.GetFileName(name));
        return File.Exists(path)
            ? new StoredFile(path, UploadRules.ContentTypeOf(Path.GetExtension(path).ToLowerInvariant()), name)
            : null;
    }

    // ================= UC04 - CV =================
    public Task<bool> HasProfileAsync(int taiKhoanId)
        => _db.UngViens.AnyAsync(u => u.MaTaiKhoan == taiKhoanId);

    public async Task<List<CvItem>> GetCvsAsync(int taiKhoanId)
        => await _db.CVUngViens.AsNoTracking()
            .Where(c => c.UngVien!.MaTaiKhoan == taiKhoanId)
            .OrderByDescending(c => c.NgayTaiLen).ThenByDescending(c => c.MaCV)
            .Select(c => new CvItem
            {
                MaCV = c.MaCV, TenTep = c.TenTep, DungLuongKB = c.DungLuongKB, NgayTaiLen = c.NgayTaiLen,
                DaDungUngTuyen = _db.DonUngTuyens.Any(d => d.MaCV == c.MaCV)
            }).ToListAsync();

    public async Task<ServiceResult<CvItem>> AddCvAsync(int taiKhoanId, IFormFile file)
    {
        var maUngVien = await _db.UngViens.AsNoTracking().Where(u => u.MaTaiKhoan == taiKhoanId)
            .Select(u => (int?)u.MaUngVien).FirstOrDefaultAsync();
        if (maUngVien == null)
            return new ServiceResult<CvItem>(false, null, "Bạn cần hoàn thiện hồ sơ trước khi tải CV.");

        var err = UploadRules.CheckCv(file, out var ext);
        if (err != null) return new ServiceResult<CvItem>(false, null, err);

        if (await _db.CVUngViens.CountAsync(c => c.MaUngVien == maUngVien) >= MaxCvPerCandidate)
            return new ServiceResult<CvItem>(false, null, $"Mỗi ứng viên chỉ lưu tối đa {MaxCvPerCandidate} CV. Hãy xóa bớt CV cũ.");

        var stored = await SaveToDiskAsync(file, CvDir, ext);   // tên ngẫu nhiên (GUID), không dùng tên người dùng đặt
        var cv = new CVUngVien
        {
            MaUngVien = maUngVien.Value,
            TenTep = SafeName(file.FileName),
            DuongDanTep = stored,                                // kết thúc bằng .pdf/.docx -> thỏa CHECK
            DungLuongKB = (int)Math.Ceiling(file.Length / 1024.0),
            NgayTaiLen = DateTime.Now
        };
        _db.CVUngViens.Add(cv);

        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            DeleteQuiet(CvDir, stored);
            return new ServiceResult<CvItem>(false, null, "Không thể lưu CV. Vui lòng thử lại.");
        }

        return new ServiceResult<CvItem>(true, new CvItem
        {
            MaCV = cv.MaCV, TenTep = cv.TenTep, DungLuongKB = cv.DungLuongKB, NgayTaiLen = cv.NgayTaiLen
        });
    }

    public async Task<ServiceResult> DeleteCvAsync(int taiKhoanId, int maCv)
    {
        // Ownership check: chỉ xóa được CV của chính mình (chống IDOR)
        var cv = await _db.CVUngViens.FirstOrDefaultAsync(c => c.MaCV == maCv && c.UngVien!.MaTaiKhoan == taiKhoanId);
        if (cv == null) return new ServiceResult(false, "Không tìm thấy CV.");

        // DonUngTuyen -> CV là Restrict: CV đã dùng ứng tuyển thì không xóa được
        if (await _db.DonUngTuyens.AnyAsync(d => d.MaCV == maCv))
            return new ServiceResult(false, "CV này đã được dùng để ứng tuyển nên không thể xóa.");

        var stored = cv.DuongDanTep;
        _db.CVUngViens.Remove(cv);
        await _db.SaveChangesAsync();
        DeleteQuiet(CvDir, stored);
        return new ServiceResult(true);
    }

    public async Task<StoredFile?> GetCvFileAsync(int taiKhoanId, int maCv)
    {
        var cv = await _db.CVUngViens.AsNoTracking()
            .Where(c => c.MaCV == maCv && c.UngVien!.MaTaiKhoan == taiKhoanId)
            .Select(c => new { c.DuongDanTep, c.TenTep }).FirstOrDefaultAsync();
        if (cv == null) return null;

        var path = Path.Combine(CvDir, Path.GetFileName(cv.DuongDanTep));
        return File.Exists(path)
            ? new StoredFile(path, UploadRules.ContentTypeOf(Path.GetExtension(path).ToLowerInvariant()), cv.TenTep)
            : null;
    }

    // ================= Helpers =================
    private static string? N(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string NormalizeSkills(string s)
        => string.Join(", ", s.Split(new[] { ',', ';', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase));

    private static string SafeName(string fileName)
    {
        var name = new string(Path.GetFileName(fileName).Where(c => !char.IsControl(c)).ToArray());
        if (name.Length > 255)
        {
            var ext = Path.GetExtension(name);
            name = name[..(255 - ext.Length)] + ext;
        }
        return name;
    }

    private static async Task<string> SaveToDiskAsync(IFormFile file, string dir, string ext)
    {
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using var fs = new FileStream(Path.Combine(dir, name), FileMode.CreateNew);
        await file.CopyToAsync(fs);
        return name;
    }

    private static void DeleteQuiet(string dir, string name)
    {
        try { File.Delete(Path.Combine(dir, Path.GetFileName(name))); } catch { /* bỏ qua */ }
    }
}