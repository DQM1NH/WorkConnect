using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers;

[Route("viec-lam")]
public class JobController : Controller
{
    private readonly IJobService _jobs;

    public JobController(IJobService jobs) => _jobs = jobs;

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] JobSearchFilter filter)
    {
        filter.Normalize();

      
        var salaryError = filter.ValidateLuong();
        if (salaryError != null) ModelState.AddModelError("LuongDenTrieu", salaryError);

        var vm = new JobSearchViewModel
        {
            Filter = filter,
            Options = await _jobs.GetFilterOptionsAsync(),
            Items = salaryError == null
                ? await _jobs.SearchAsync(filter)
                : new List<JobListItem>()
        };
        return View(vm);
    }


    [HttpGet("tim-kiem")]
    public async Task<IActionResult> Search([FromQuery] JobSearchFilter filter)
    {
        filter.Normalize();

        var salaryError = filter.ValidateLuong();
        if (salaryError != null) return BadRequest(salaryError);   

        var items = await _jobs.SearchAsync(filter);
        return PartialView("_JobList", items);
    }

    
    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var job = await _jobs.GetDetailAsync(slug);
        if (job == null) return NotFound();
        return View(job);
    }
}