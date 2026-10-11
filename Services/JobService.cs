using Microsoft.EntityFrameworkCore;
using WorkConnect.Data;
using WorkConnect.Models;

namespace WorkConnect.Services;

public interface IJobService
{
    Task<JobFilterOptions> GetFilterOptionsAsync();
    Task<List<JobListItem>> SearchAsync(JobSearchFilter filter);
    Task<JobDetailViewModel?> GetDetailAsync(string slug);
}

public class JobService : IJobService
{
    private const string DangTuyen = "Đang tuyển";
    private const string BanNhap = "Bản nháp";

    private readonly ApplicationDbContext _db;

    public JobService(ApplicationDbContext db) => _db = db;

    public async Task<JobFilterOptions> GetFilterOptionsAsync()
    {
        var options = new JobFilterOptions();

        options.NganhNghes = await _db.NganhNghes.AsNoTracking()
            .OrderBy(x => x.MaNganhNghe).Select(x => new OptionItem(x.MaNganhNghe, x.TenNganhNghe)).ToListAsync();
        options.DiaDiems = await _db.DiaDiems.AsNoTracking()
            .OrderBy(x => x.MaDiaDiem).Select(x => new OptionItem(x.MaDiaDiem, x.TenDiaDiem)).ToListAsync();
        options.LoaiHinhs = await _db.LoaiHinhCongViecs.AsNoTracking()
            .OrderBy(x => x.MaLoaiHinh).Select(x => new OptionItem(x.MaLoaiHinh, x.TenLoaiHinh)).ToListAsync();
        options.MucKinhNghiems = await _db.MucKinhNghiems.AsNoTracking()
            .OrderBy(x => x.MaMucKinhNghiem).Select(x => new OptionItem(x.MaMucKinhNghiem, x.TenMucKinhNghiem)).ToListAsync();
        options.HinhThucs = await _db.HinhThucLamViecs.AsNoTracking()
            .OrderBy(x => x.MaHinhThuc).Select(x => new OptionItem(x.MaHinhThuc, x.TenHinhThuc)).ToListAsync();

        return options;
    }

    public async Task<List<JobListItem>> SearchAsync(JobSearchFilter f)
    {
        f.Normalize();
        var today = DateTime.Today;

        // GIẢ ĐỊNH (báo cáo chưa nêu): danh sách chỉ hiện tin "Đang tuyển" và chưa quá hạn.
        var q = _db.TinTuyenDungs.AsNoTracking()
            .Where(t => t.TrangThai == DangTuyen && t.HanUngTuyen >= today);

        // R03: từ khóa tìm theo tiêu đề, vị trí hoặc tên doanh nghiệp
        if (f.Keyword is string kw)
        {
            q = q.Where(t => t.TieuDe.Contains(kw)
                          || t.ViTri.Contains(kw)
                          || t.NhaTuyenDung!.TenCongTy.Contains(kw));
        }

        // R04: các tiêu chí lọc kết hợp được với nhau
        if (f.MaNganhNghe is int nn) q = q.Where(t => t.MaNganhNghe == nn);
        if (f.MaDiaDiem is int dd) q = q.Where(t => t.MaDiaDiem == dd);
        if (f.MaLoaiHinh is int lh) q = q.Where(t => t.MaLoaiHinh == lh);
        if (f.MaHinhThuc is int ht) q = q.Where(t => t.MaHinhThuc == ht);
        if (f.MaMucKinhNghiem is int kn) q = q.Where(t => t.MaMucKinhNghiem == kn);

        // GIẢ ĐỊNH (báo cáo chưa nêu): khi lọc lương, tin "Thỏa thuận" bị loại;
        // tin có mức lương được giữ lại nếu khoảng lương của tin giao với khoảng người dùng chọn.
        if (f.LuongTuTrieu is decimal tu)
        {
            var min = tu * 1_000_000m;
            q = q.Where(t => !t.LuongThoaThuan && (t.LuongToiDa ?? t.LuongToiThieu) >= min);
        }
        if (f.LuongDenTrieu is decimal den)
        {
            var max = den * 1_000_000m;
            q = q.Where(t => !t.LuongThoaThuan && (t.LuongToiThieu ?? t.LuongToiDa) <= max);
        }

        // Sắp xếp theo BM07
        q = f.Sort switch
        {
            JobSort.LuongCao => q
                .OrderByDescending(t => t.LuongThoaThuan ? 0m : (t.LuongToiDa ?? t.LuongToiThieu ?? 0m))
                .ThenByDescending(t => t.NgayDang),
            JobSort.SapHetHan => q
                .OrderBy(t => t.HanUngTuyen)
                .ThenByDescending(t => t.NgayDang),
            _ => q.OrderByDescending(t => t.NgayDang)
        };

        // Một truy vấn duy nhất, chỉ lấy các cột cần hiển thị (R25)
        return await q.Select(t => new JobListItem
        {
            MaTin = t.MaTin,
            TieuDe = t.TieuDe,
            DuongDanThanThien = t.DuongDanThanThien,
            ViTri = t.ViTri,
            TenCongTy = t.NhaTuyenDung!.TenCongTy,
            DuongDanLogo = t.NhaTuyenDung!.DuongDanLogo,
            DiaDiem = t.DiaDiem!.TenDiaDiem,
            NganhNghe = t.NganhNghe!.TenNganhNghe,
            LoaiHinh = t.LoaiHinh!.TenLoaiHinh,
            HinhThuc = t.HinhThuc!.TenHinhThuc,
            KinhNghiem = t.MucKinhNghiem != null ? t.MucKinhNghiem.TenMucKinhNghiem : null,
            LuongToiThieu = t.LuongToiThieu,
            LuongToiDa = t.LuongToiDa,
            LuongThoaThuan = t.LuongThoaThuan,
            HanUngTuyen = t.HanUngTuyen
        }).ToListAsync();
    }

    public async Task<JobDetailViewModel?> GetDetailAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        // GIẢ ĐỊNH: tin "Bản nháp" không công khai; tin "Đã đóng"/quá hạn vẫn xem được nhưng không ứng tuyển được.
        return await _db.TinTuyenDungs.AsNoTracking()
            .Where(t => t.DuongDanThanThien == slug && t.TrangThai != BanNhap)
            .Select(t => new JobDetailViewModel
            {
                MaTin = t.MaTin,
                TieuDe = t.TieuDe,
                ViTri = t.ViTri,
                NganhNghe = t.NganhNghe!.TenNganhNghe,
                DiaDiem = t.DiaDiem!.TenDiaDiem,
                LoaiHinh = t.LoaiHinh!.TenLoaiHinh,
                HinhThuc = t.HinhThuc!.TenHinhThuc,
                KinhNghiem = t.MucKinhNghiem != null ? t.MucKinhNghiem.TenMucKinhNghiem : null,
                LuongToiThieu = t.LuongToiThieu,
                LuongToiDa = t.LuongToiDa,
                LuongThoaThuan = t.LuongThoaThuan,
                SoLuongTuyen = t.SoLuongTuyen,
                MoTaCongViec = t.MoTaCongViec,
                YeuCauCongViec = t.YeuCauCongViec,
                YeuCauUngVien = t.YeuCauUngVien,
                QuyenLoi = t.QuyenLoi,
                NgayDang = t.NgayDang,
                HanUngTuyen = t.HanUngTuyen,
                TrangThai = t.TrangThai,
                TenCongTy = t.NhaTuyenDung!.TenCongTy,
                DuongDanLogo = t.NhaTuyenDung!.DuongDanLogo,
                DiaChiCongTy = t.NhaTuyenDung!.DiaChi,
                Website = t.NhaTuyenDung!.Website,
                GioiThieuCongTy = t.NhaTuyenDung!.GioiThieu
            })
            .FirstOrDefaultAsync();
    }
}