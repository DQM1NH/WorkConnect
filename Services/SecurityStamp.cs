using System.Security.Cryptography;
using System.Text;

namespace WorkConnect.Services;

/// <summary>
/// "Dấu bảo mật" suy ra từ chuỗi băm mật khẩu: đổi/đặt lại mật khẩu thì dấu đổi theo,
/// các cookie đăng nhập cũ bị vô hiệu ở lần request kế tiếp (không cần thêm cột CSDL).
/// </summary>
public static class SecurityStamp
{
    public const string ClaimType = "wc:stamp";

    public static string From(string? passwordHash)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash ?? string.Empty));
        return Convert.ToHexString(bytes, 0, 16);
    }
}
