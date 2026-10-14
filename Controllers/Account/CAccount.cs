using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;
using WorkConnect.Models.Entities;

namespace WorkConnect.Controllers
{
    public class AccountController : Controller
    {
        private readonly WorkConnectDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _hasher;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            WorkConnectDbContext context,
            IPasswordHasher<TaiKhoan> hasher,
            ILogger<AccountController> logger)
        {
            _context = context;
            _hasher = hasher;
            _logger = logger;
        }

        // ===================== ĐĂNG NHẬP =====================

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);

            var verify = user == null
                ? PasswordVerificationResult.Failed
                : VerifyPassword(user, model.Password);

            // Dùng chung 1 thông báo cho "sai email" và "sai mật khẩu" để không lộ email nào đã tồn tại
            if (user == null || verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            // Chỉ báo khóa sau khi đã xác thực đúng mật khẩu
            if (user.BiKhoa)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên");
                return View(model);
            }

            // Thuật toán băm đã được nâng cấp -> băm lại mật khẩu bằng chuẩn mới
            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.MatKhauHash = _hasher.HashPassword(user, model.Password);
                await _context.SaveChangesAsync();
            }

            await SignInUserAsync(user, model.RememberMe);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return user.MaVaiTro == AppRoles.AdminId
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        // ===================== ĐĂNG KÝ =====================

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);

            if (await _context.TaiKhoans.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký");
                return View(model);
            }

            var user = new TaiKhoan
            {
                MaVaiTro = AppRoles.UngVienId,   // đăng ký công khai luôn là Ứng viên
                Email = email,
                HoTen = $"{model.LastName.Trim()} {model.FirstName.Trim()}",
                SoDienThoai = model.PhoneNumber.Trim(),
                BiKhoa = false,
                NgayTao = DateTime.Now
            };
            user.MatKhauHash = _hasher.HashPassword(user, model.Password);

            _context.TaiKhoans.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Không thể tạo tài khoản cho {Email}", email);

                // Trường hợp 2 người đăng ký cùng email cùng lúc (khi đã có unique index)
                if (await _context.TaiKhoans.AnyAsync(u => u.Email == email))
                    ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký");
                else
                    ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản lúc này. Vui lòng thử lại sau");

                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        // ===================== ĐĂNG XUẤT / TỪ CHỐI TRUY CẬP =====================

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ===================== HELPERS =====================

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        /// <summary>
        /// Xác thực mật khẩu. Hỗ trợ cả tài khoản cũ đang lưu mật khẩu dạng thô (dữ liệu mẫu trong CSDL):
        /// nếu khớp thì trả về SuccessRehashNeeded để Login tự băm lại và ghi đè.
        /// </summary>
        private PasswordVerificationResult VerifyPassword(TaiKhoan user, string password)
        {
            var stored = user.MatKhauHash;
            if (string.IsNullOrEmpty(stored))
                return PasswordVerificationResult.Failed;

            // Chuỗi băm của PasswordHasher (định dạng v3) luôn bắt đầu bằng "AQAAAA"
            if (!stored.StartsWith("AQAAAA", StringComparison.Ordinal))
            {
                var a = Encoding.UTF8.GetBytes(stored);
                var b = Encoding.UTF8.GetBytes(password);
                return CryptographicOperations.FixedTimeEquals(a, b)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }

            return _hasher.VerifyHashedPassword(user, stored, password);
        }

        private async Task SignInUserAsync(TaiKhoan user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString()),
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.HoTen) ? user.Email : user.HoTen),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, AppRoles.FromId(user.MaVaiTro))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var props = new AuthenticationProperties
            {
                IsPersistent = rememberMe,                                   // "Tự động đăng nhập"
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props);
        }
    }
}
