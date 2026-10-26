
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Data;
using WorkConnect.Models;
using WorkConnect.Models.Entities;

namespace WorkConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<TaiKhoan> _hasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== ĐĂNG NHẬP ====================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();

            var user = await _context.TaiKhoans
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (user.BiKhoa)
            {
                ModelState.AddModelError(
                    "", "Tài khoản đã bị khóa.");
                return View(model);
            }

            // Kiểm tra mật khẩu đã được băm theo chuẩn ASP.NET Core.
            var verify = user.MatKhauMaHoa.StartsWith(
                "AQAAAA", StringComparison.Ordinal)
                ? _hasher.VerifyHashedPassword(
                    user, user.MatKhauMaHoa, model.Password)
                : PasswordVerificationResult.Failed;

            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.MatKhauMaHoa =
                    _hasher.HashPassword(user, model.Password);

                await _context.SaveChangesAsync();
            }

            var role = user.MaVaiTro switch
            {
                1 => "Admin",
                2 => "Employer",
                3 => "Candidate",
                _ => ""
            };

            if (string.IsNullOrEmpty(role))
            {
                ModelState.AddModelError(
                    "", "Vai trò tài khoản không hợp lệ.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.MaTaiKhoan.ToString()),

                new(
                    ClaimTypes.Name,
                    string.IsNullOrWhiteSpace(user.HoTen)
                        ? user.Email
                        : user.HoTen),

                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (role == "Employer")
                return RedirectToAction("Index", "Employer");

            return RedirectToAction("Index", "Home");
        }

        // ==================== ĐĂNG KÝ ====================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim().ToLowerInvariant();

            // Chỉ chấp nhận hai loại tài khoản hợp lệ.
            if (model.Role != "Candidate" && model.Role != "Employer")
            {
                ModelState.AddModelError(nameof(model.Role),
                    "Loại tài khoản không hợp lệ.");
                return View(model);
            }

            // Kiểm tra email đã được sử dụng chưa.
            var emailExists = await _context.TaiKhoans
                .AnyAsync(x => x.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email này đã được sử dụng.");
                return View(model);
            }

            // Kiểm tra thông tin riêng của nhà tuyển dụng.
            if (model.Role == "Employer")
            {
                if (string.IsNullOrWhiteSpace(model.TenCongTy))
                    ModelState.AddModelError(nameof(model.TenCongTy),
                        "Vui lòng nhập tên công ty.");

                if (string.IsNullOrWhiteSpace(model.DiaChiCongTy))
                    ModelState.AddModelError(nameof(model.DiaChiCongTy),
                        "Vui lòng nhập địa chỉ công ty.");

                if (string.IsNullOrWhiteSpace(model.SoDienThoaiCongTy))
                    ModelState.AddModelError(nameof(model.SoDienThoaiCongTy),
                        "Vui lòng nhập số điện thoại công ty.");

                if (model.MaNganhNghe == null ||
                    !await _context.NganhNghes
                        .AnyAsync(x => x.MaNganhNghe == model.MaNganhNghe.Value))
                {
                    ModelState.AddModelError(nameof(model.MaNganhNghe),
                        "Vui lòng chọn ngành nghề hợp lệ.");
                }

                if (model.MaQuyMo.HasValue &&
                    !await _context.QuyMoCongTies
                        .AnyAsync(x => x.MaQuyMo == model.MaQuyMo.Value))
                {
                    ModelState.AddModelError(nameof(model.MaQuyMo),
                        "Quy mô công ty không hợp lệ.");
                }

                if (!string.IsNullOrWhiteSpace(model.WebsiteCongTy) &&
                    !model.WebsiteCongTy.StartsWith("https://",
                        StringComparison.OrdinalIgnoreCase) &&
                    !model.WebsiteCongTy.StartsWith("http://",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(model.WebsiteCongTy),
                        "Website phải bắt đầu bằng http:// hoặc https://.");
                }

                if (string.IsNullOrWhiteSpace(model.GioiThieuCongTy))
                    ModelState.AddModelError(nameof(model.GioiThieuCongTy),
                        "Vui lòng giới thiệu công ty.");

                if (!ModelState.IsValid)
                    return View(model);
            }

            // Tạo tài khoản theo vai trò đã chọn.
            var user = new TaiKhoan
            {
                MaVaiTro = model.Role == "Employer" ? 2 : 3,
                Email = email,
                HoTen = $"{model.FirstName.Trim()} {model.LastName.Trim()}",
                MatKhauMaHoa = string.Empty,

                // Giữ số điện thoại hợp lệ để đáp ứng CHECK của bảng TaiKhoan.
                // Số liên hệ công ty được lưu riêng trong hồ sơ nhà tuyển dụng.
                SoDienThoai = "0900000000",

                BiKhoa = false,
                NgayTao = DateTime.Now
            };

            user.MatKhauMaHoa =
                _hasher.HashPassword(user, model.Password);

            // Tạo tài khoản và hồ sơ công ty trong cùng transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.TaiKhoans.Add(user);
                await _context.SaveChangesAsync();

                if (model.Role == "Employer")
                {
                    var employer = new NhaTuyenDung
                    {
                        MaTaiKhoan = user.MaTaiKhoan,
                        TenCongTy = model.TenCongTy!.Trim(),
                        EmailLienHe = email,
                        SoDienThoaiLienHe =
                            model.SoDienThoaiCongTy!.Trim(),
                        DiaChi = model.DiaChiCongTy!.Trim(),
                        Website = string.IsNullOrWhiteSpace(model.WebsiteCongTy)
                            ? null
                            : model.WebsiteCongTy.Trim(),
                        MaNganhNghe = model.MaNganhNghe!.Value,
                        MaQuyMo = model.MaQuyMo,
                        GioiThieu = model.GioiThieuCongTy!.Trim()
                    };

                    _context.NhaTuyenDungs.Add(employer);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("",
                    "Không thể đăng ký tài khoản. Vui lòng kiểm tra thông tin và thử lại.");
                return View(model);
            }

            TempData["SuccessMessage"] =
                "Đăng ký thành công. Bạn có thể đăng nhập.";

            return RedirectToAction("Login");
        }


        

        // ==================== ĐĂNG XUẤT ====================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}
