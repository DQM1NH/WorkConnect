using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;
using WorkConnect.Services;

namespace WorkConnect.Controllers;

/// <summary>UC03 Hồ sơ, UC04 CV, UC08 Theo dõi đơn ứng tuyển.</summary>
[Authorize(Roles = AppRoles.UngVien)]
public class CandidateController : Controller
{
    private readonly ICandidateService _candidates;
    private readonly IApplicationService _applications;

    public CandidateController(ICandidateService candidates, IApplicationService applications)
    { _candidates = candidates; _applications = applications; }

    // ---------------- UC03: Hồ sơ ----------------
    [HttpGet]
    public async Task<IActionResult> Profile(string? returnUrl = null)
    {
        var vm = await _candidates.GetProfileAsync(User.GetAccountId());
        vm.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(UploadRules.RequestLimit)]
    public async Task<IActionResult> Profile([Bind(Prefix = "Form")] ProfileForm form, IFormFile? avatar, string? returnUrl = null)
    {
        var id = User.GetAccountId();

        if (ModelState.IsValid)
        {
            var result = await _candidates.SaveProfileAsync(id, form, avatar);
            if (result.Ok)
            {
                TempData["Success"] = "Đã lưu hồ sơ.";
                if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);   // quay lại trang ứng tuyển nếu có
                return RedirectToAction(nameof(Profile));
            }
            ModelState.AddModelError(string.Empty, result.Error!);
        }

        var vm = await _candidates.GetProfileAsync(id);
        vm.Form = form;                                       // giữ dữ liệu người dùng vừa nhập
        vm.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        return View(vm);
    }

    // Ảnh đại diện của chính mình (file nằm ngoài wwwroot)
    [HttpGet]
    public async Task<IActionResult> Avatar()
    {
        var f = await _candidates.GetAvatarAsync(User.GetAccountId());
        if (f == null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(f.PhysicalPath, f.ContentType);
    }

    // ---------------- UC04: CV ----------------
    [HttpGet]
    public async Task<IActionResult> Cv()
    {
        var id = User.GetAccountId();
        return View(new CvPageViewModel
        {
            DaCoHoSo = await _candidates.HasProfileAsync(id),
            Cvs = await _candidates.GetCvsAsync(id)
        });
    }

    // AJAX: tải CV lên, trả về phần HTML danh sách CV
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(UploadRules.RequestLimit)]
    public async Task<IActionResult> UploadCv(IFormFile? file)
    {
        if (file == null) return BadRequest(new { error = "Vui lòng chọn tệp CV." });
        var id = User.GetAccountId();
        var r = await _candidates.AddCvAsync(id, file);
        if (!r.Ok) return BadRequest(new { error = r.Error });
        return PartialView("_CvList", new CvPageViewModel
        {
            DaCoHoSo = await _candidates.HasProfileAsync(id),
            Cvs = await _candidates.GetCvsAsync(id)
        });
    }

    // AJAX: xóa CV
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCv(int maCv)
    {
        var id = User.GetAccountId();
        var r = await _candidates.DeleteCvAsync(id, maCv);
        if (!r.Ok) return BadRequest(new { error = r.Error });
        return PartialView("_CvList", new CvPageViewModel
        {
            DaCoHoSo = await _candidates.HasProfileAsync(id),
            Cvs = await _candidates.GetCvsAsync(id)
        });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCv(int id)
    {
        var f = await _candidates.GetCvFileAsync(User.GetAccountId(), id);
        if (f == null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(f.PhysicalPath, f.ContentType, f.DownloadName);   // tải về dạng attachment
    }

    // ---------------- UC08: Theo dõi đơn ----------------
    [HttpGet]
    public async Task<IActionResult> Applications(int? status = null)
        => View(await _applications.GetMineAsync(User.GetAccountId(), status));

    // AJAX: lọc theo trạng thái, chỉ trả về phần danh sách
    [HttpGet]
    public async Task<IActionResult> ApplicationsList(int? status = null)
    {
        var vm = await _applications.GetMineAsync(User.GetAccountId(), status);
        return PartialView("_ApplicationList", vm.Items);
    }
}