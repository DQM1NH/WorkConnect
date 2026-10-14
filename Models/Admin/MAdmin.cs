namespace WorkConnect.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalApplications { get; set; }

        public List<JobPendingViewModel> PendingJobs { get; set; } = new();
        public List<UserAdminViewModel> Users { get; set; } = new();
        public List<ContentAdminViewModel> Content { get; set; } = new();
    }

    public class JobPendingViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string EmployerName { get; set; }
        public string Category { get; set; }
        public DateTime PostedDate { get; set; }
        public string SalaryRange { get; set; }
    }

    public class UserAdminViewModel
    {
        public int RawId { get; set; } // Khóa chính MaTaiKhoan
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ContentAdminViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public string Salary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public bool IsHidden { get; set; } 
    }
}