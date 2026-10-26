using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers;

[Authorize(Roles = "Candidate")]
[Route("ung-tuyen")]
public class ApplicationController : Controller
{
    private readonly IApplicationService _apps;

    public ApplicationController(IApplicationService apps) => _apps = apps;

    // GET /ung-tuyen/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> Apply(string slug)
    {
        var r = await _apps.PrepareAsync(User.GetAccountId(), slug);
        if (r.Check != ApplyCheck.Ready) return RedirectByCheck(r.Check, slug);
        return View(r.Model);
    }

    // POST /ung-tuyen/{slug}
    [HttpPost("{slug}"), ValidateAntiForgeryToken, RequestSizeLimit(UploadRules.RequestLimit)]
    public async Task<IActionResult> Apply(string slug, [Bind(Prefix = "Form")] ApplyForm form, IFormFile? newCv)
    {
        if (!ModelState.IsValid) return await Redisplay(slug, form, null);

        var r = await _apps.SubmitAsync(User.GetAccountId(), slug, form, newCv);
        if (r.Ok)
        {
            TempData["Success"] = "Đã gửi đơn ứng tuyển. Bạn có thể theo dõi trạng thái tại mục Đơn ứng tuyển.";
            return RedirectToAction("Applications", "Candidate");
        }
        if (r.Check == ApplyCheck.Invalid) return await Redisplay(slug, form, r.Error);
        return RedirectByCheck(r.Check, slug);
    }

    private async Task<IActionResult> Redisplay(string slug, ApplyForm form, string? error)
    {
        var r = await _apps.PrepareAsync(User.GetAccountId(), slug);
        if (r.Check != ApplyCheck.Ready) return RedirectByCheck(r.Check, slug);
        if (error != null) ModelState.AddModelError(string.Empty, error);
        r.Model!.Form = form;
        return View(r.Model);
    }

    private IActionResult RedirectByCheck(ApplyCheck check, string slug)
    {
        if (check == ApplyCheck.JobNotFound) return NotFound();
        if (check == ApplyCheck.JobClosed)
        {
            TempData["Error"] = "Tin tuyển dụng đã đóng hoặc đã hết hạn ứng tuyển.";
            return RedirectToAction("Details", "Job", new { slug });
        }
        if (check == ApplyCheck.AlreadyApplied)
        {
            TempData["Error"] = "Bạn đã ứng tuyển tin này rồi.";
            return RedirectToAction("Applications", "Candidate");
        }
        // NoProfile: chuyển sang hồ sơ, lưu xong quay lại đúng trang ứng tuyển
        TempData["Error"] = "Vui lòng hoàn thiện hồ sơ trước khi ứng tuyển.";
        return RedirectToAction("Profile", "Candidate", new { returnUrl = Url.Action("Apply", "Application", new { slug }) });
    }
}