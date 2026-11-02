using System.IO.Compression;

namespace WorkConnect.Services;

public static class UploadRules
{
    public const long MaxCvBytes = 5L * 1024 * 1024;       // 5 MB = 5120 KB, khớp CHECK của DB
    public const long MaxAvatarBytes = 2L * 1024 * 1024;   // 2 MB
    public const long RequestLimit = 7L * 1024 * 1024;     // giới hạn cả request

    public static string ContentTypeOf(string ext) => ext switch
    {
        ".pdf" => "application/pdf",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".png" => "image/png",
        ".jpg" => "image/jpeg",
        _ => "application/octet-stream"
    };

    /// <summary>Trả về thông báo lỗi, hoặc null nếu CV hợp lệ.</summary>
    public static string? CheckCv(IFormFile f, out string ext)
    {
        ext = Path.GetExtension(f.FileName).ToLowerInvariant();
        if (f.Length <= 0) return "Tệp CV rỗng.";
        if (f.Length > MaxCvBytes) return "CV vượt quá dung lượng tối đa 5 MB.";
        if (ext != ".pdf" && ext != ".docx") return "Chỉ chấp nhận CV định dạng PDF hoặc DOCX.";

        using var s = f.OpenReadStream();
        var head = new byte[5];
        var n = s.Read(head, 0, head.Length);

        if (ext == ".pdf")   // "%PDF-"
            return n == 5 && head[0] == 0x25 && head[1] == 0x50 && head[2] == 0x44 && head[3] == 0x46 && head[4] == 0x2D
                ? null : "Nội dung tệp không phải PDF hợp lệ.";

        // .docx là file ZIP ("PK\x03\x04") có chứa word/document.xml
        if (n < 4 || head[0] != 0x50 || head[1] != 0x4B || head[2] != 0x03 || head[3] != 0x04)
            return "Nội dung tệp không phải DOCX hợp lệ.";
        try
        {
            s.Position = 0;
            using var zip = new ZipArchive(s, ZipArchiveMode.Read, leaveOpen: true);
            return zip.GetEntry("[Content_Types].xml") != null && zip.GetEntry("word/document.xml") != null
                ? null : "Nội dung tệp không phải DOCX hợp lệ.";
        }
        catch (InvalidDataException) { return "Nội dung tệp không phải DOCX hợp lệ."; }
    }

    public static string? CheckAvatar(IFormFile f, out string ext)
    {
        ext = Path.GetExtension(f.FileName).ToLowerInvariant();
        if (ext == ".jpeg") ext = ".jpg";
        if (f.Length <= 0) return "Ảnh đại diện rỗng.";
        if (f.Length > MaxAvatarBytes) return "Ảnh đại diện vượt quá 2 MB.";
        if (ext != ".jpg" && ext != ".png") return "Ảnh đại diện chỉ chấp nhận JPG hoặc PNG.";

        using var s = f.OpenReadStream();
        var h = new byte[8];
        var n = s.Read(h, 0, h.Length);
        var jpg = n >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        var png = n == 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47
                         && h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A;
        return (ext == ".jpg" ? jpg : png) ? null : "Nội dung tệp không phải ảnh JPG/PNG hợp lệ.";
    }
}