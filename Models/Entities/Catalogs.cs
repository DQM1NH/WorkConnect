using System.ComponentModel.DataAnnotations;

namespace WorkConnect.Models.Entities;

// Các bảng danh mục - Bảng 3.20 (cột Ten... đều NVARCHAR, NOT NULL, UNIQUE)

public class VaiTro
{
    [Key] public int MaVaiTro { get; set; }
    public string TenVaiTro { get; set; } = string.Empty;   // Admin, Employer, Candidate (Bảng 3.19)
}

public class TrangThaiDon
{
    [Key] public int MaTrangThai { get; set; }
    public string TenTrangThai { get; set; } = string.Empty;
}

public class LoaiHinhCongViec
{
    [Key] public int MaLoaiHinh { get; set; }
    public string TenLoaiHinh { get; set; } = string.Empty;
}

public class HinhThucLamViec
{
    [Key] public int MaHinhThuc { get; set; }
    public string TenHinhThuc { get; set; } = string.Empty;
}

public class MucKinhNghiem
{
    [Key] public int MaMucKinhNghiem { get; set; }
    public string TenMucKinhNghiem { get; set; } = string.Empty;
}

public class TrinhDoHocVan
{
    [Key] public int MaTrinhDo { get; set; }
    public string TenTrinhDo { get; set; } = string.Empty;
}

public class QuyMoCongTy
{
    [Key] public int MaQuyMo { get; set; }
    public string TenQuyMo { get; set; } = string.Empty;
}

