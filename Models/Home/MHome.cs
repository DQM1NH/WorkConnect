namespace WorkConnect.Models
{
    public record CategoryCount(int Id, string Name, int Count);
    public record EmployerCard(int Id, string Name, string? Logo, int JobCount);

    public class HomeViewModel
    {
        public JobFilterOptions Options { get; set; } = new();
        public List<JobListItem> Latest { get; set; } = new();
        public List<CategoryCount> Categories { get; set; } = new();
        public List<EmployerCard> Employers { get; set; } = new();
        public int TotalJobs { get; set; }
        public int TotalEmployers { get; set; }
    }
}