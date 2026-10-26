using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers;

/// <summary>UC05 - Tìm kiếm và lọc việc làm; UC06 - Xem chi tiết tin tuyển dụng.</summary>
[Route("viec-lam")]
public class JobController : Controller
{
    private readonly IJobService _jobs;

    public JobController(IJobService jobs) => _jobs = jobs;

    // GET /viec-lam  (hiển thị cả trang, kể cả khi trình duyệt tắt JavaScript)
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] JobSearchFilter filter)
    {
        filter.Normalize();
        var vm = new JobSearchViewModel
        {
            Filter = filter,
            Options = await _jobs.GetFilterOptionsAsync(),
            Items = await _jobs.SearchAsync(filter)
        };
        return View(vm);
    }

    // GET /viec-lam/tim-kiem  (AJAX: chỉ trả về phần danh sách kết quả)
    [HttpGet("tim-kiem")]
    public async Task<IActionResult> Search([FromQuery] JobSearchFilter filter)
    {
        filter.Normalize();
        var items = await _jobs.SearchAsync(filter);
        return PartialView("_JobList", items);
    }

    // GET /viec-lam/{slug}  (URL thân thiện lấy từ cột DuongDanThanThien)
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var job = await _jobs.GetDetailAsync(slug);
        if (job == null) return NotFound();
        return View(job);
    }
}