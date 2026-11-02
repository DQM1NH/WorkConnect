
using WorkConnect.Models.Entities;

namespace WorkConnect.Models.Employer
{
    public class EmployerDashboardViewModel
    {
        public int TotalJobs { get; set; }
        public int RecruitingJobs { get; set; }
        public int PendingJobs { get; set; }
        public int ExpiredJobs { get; set; }
        public int TotalApplications { get; set; }

        public List<TinTuyenDung> RecentJobs { get; set; }
            = new List<TinTuyenDung>();
    }
}
