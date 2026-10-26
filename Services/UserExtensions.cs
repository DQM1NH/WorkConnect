using System.Security.Claims;

namespace WorkConnect.Services;

public static class UserExtensions
{
    /// <summary>MaTaiKhoan của người đang đăng nhập (0 nếu chưa đăng nhập).</summary>
    public static int GetAccountId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
}