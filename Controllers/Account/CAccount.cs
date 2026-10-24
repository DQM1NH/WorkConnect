
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

            // Kiểm tra email đã tồn tại.
            var emailExists = await _context.TaiKhoans
                .AnyAsync(x => x.Email.ToLower() == email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email này đã được sử dụng.");

                return View(model);
            }

            // Tạo tài khoản ứng viên.
            
            var user = new TaiKhoan
            {
                MaVaiTro = 3,
                Email = email,
                HoTen = $"{model.FirstName.Trim()} {model.LastName.Trim()}",
                MatKhauMaHoa = string.Empty,
                SoDienThoai = "0900000000",
                BiKhoa = false,
                NgayTao = DateTime.Now
            };

            // Băm mật khẩu trước khi lưu.
            user.MatKhauMaHoa = _hasher.HashPassword(
                user, model.Password);

            _context.TaiKhoans.Add(user);
            await _context.SaveChangesAsync();

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
