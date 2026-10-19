using WorkConnect.Data;

namespace WorkConnect.Services;

// Bản tạm để build được. Bước 5 sẽ thay bằng bản đầy đủ.
public interface ICandidateService
{
}

public class CandidateService : ICandidateService
{
    private readonly ApplicationDbContext _db;

    public CandidateService(ApplicationDbContext db) => _db = db;
}