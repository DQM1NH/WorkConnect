namespace WorkConnect.Models
{
    /// <summary>Ánh xạ tbl_TaiKhoan.MaVaiTro sang tên role dùng trong [Authorize(Roles = ...)]</summary>
    public static class AppRoles
    {
        public const int AdminId = 1;
        public const int NhaTuyenDungId = 2;
        public const int UngVienId = 3;

        public const string Admin = "Admin";
        public const string NhaTuyenDung = "NhaTuyenDung";
        public const string UngVien = "UngVien";

        public static string FromId(int maVaiTro) => maVaiTro switch
        {
            AdminId => Admin,
            NhaTuyenDungId => NhaTuyenDung,
            _ => UngVien
        };
    }
}
