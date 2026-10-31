
using WorkConnect.Models.Entities;

namespace WorkConnect.Models.Employer
{
    public class CandidateSearchViewModel
    {
        public string? Keyword { get; set; }

        public string? ChuyenNganh { get; set; }

        // Các chuyên ngành được lấy từ dữ liệu ứng viên hiện có trong DB.
        public List<string> ChuyenNganhOptions { get; set; } = new();

        public string? KyNang { get; set; }

        public string? KinhNghiem { get; set; }

        public List<UngVien> Candidates { get; set; } = new();
    }
}
