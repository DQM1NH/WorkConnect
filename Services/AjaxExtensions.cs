namespace WorkConnect.Services;

public static class AjaxExtensions
{
    public static bool IsAjaxRequest(this HttpRequest request)
        => string.Equals(request.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
