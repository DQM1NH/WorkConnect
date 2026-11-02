using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers;

public class HomeController : Controller
{
    private readonly IJobService _jobService;
    public HomeController(IJobService jobService)
    {
        _jobService = jobService;
    }

    public async Task<IActionResult> Index() => View(await _jobService.GetHomeAsync());

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}