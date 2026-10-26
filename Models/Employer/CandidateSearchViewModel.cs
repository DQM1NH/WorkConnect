
using WorkConnect.Models.Entities;

namespace WorkConnect.Models.Employer
{
    public class CandidateSearchViewModel
    {
        public string? Keyword { get; set; }

        public string? ChuyenNganh { get; set; }

        public string? KyNang { get; set; }

        public string? KinhNghiem { get; set; }

        public List<UngVien> Candidates { get; set; } = new();
    }
}
