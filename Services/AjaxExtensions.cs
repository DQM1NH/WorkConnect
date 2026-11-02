namespace WorkConnect.Services;

public static class AjaxExtensions
{
    /// <summary>
    /// Request do jQuery $.ajax / XMLHttpRequest gửi lên (có header X-Requested-With: XMLHttpRequest).
    /// Dùng để controller trả JSON / partial view thay vì redirect / trang đầy đủ.
    /// </summary>
    public static bool IsAjaxRequest(this HttpRequest request)
        => string.Equals(request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
