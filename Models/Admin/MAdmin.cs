namespace WorkConnect.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalApplications { get; set; }
        public string ActiveTab { get; set; } = "approval";

        public PagedResult<JobPendingViewModel> PendingJobs { get; set; } = new();
        public PagedResult<UserAdminViewModel> Users { get; set; } = new();
        public PagedResult<ContentAdminViewModel> Content { get; set; } = new();
        public PagedResult<ApplicationAdminViewModel> Applications { get; set; } = new();
    }

    public class JobPendingViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string EmployerName { get; set; }
        public string Category { get; set; }
        public DateTime PostedDate { get; set; }
        public string SalaryRange { get; set; }

        // Thông tin hiển thị trong modal "Xem chi tiết"
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string EmployerAddress { get; set; } = string.Empty;
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

    public class ApplicationAdminViewModel
    {
        public int ApplicationId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string CandidatePhone { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string EmployerName { get; set; } = string.Empty;
        public DateTime AppliedDate { get; set; }
    }
}