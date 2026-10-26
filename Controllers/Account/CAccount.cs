using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WorkConnect.Models;
using WorkConnect.Models.Entities;
using WorkConnect.Services;
using WorkConnect.ViewModels;

namespace WorkConnect.Controllers
{
    public class AccountController : Controller
    {
        private const int OtpMaxAttempts = 5;                                        // số lần nhập sai tối đa cho 1 mã OTP
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(5);      // OTP hết hạn sau 5 phút
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60); // khoảng cách tối thiểu giữa 2 lần gửi OTP
        private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(10);

        private readonly WorkConnectDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _hasher;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        // Chuỗi băm giả để Login tốn thời gian như nhau dù email có tồn tại hay không
        private static string? _dummyHash;

        public AccountController(
            WorkConnectDbContext context,
            IPasswordHasher<TaiKhoan> hasher,
            ILogger<AccountController> logger,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration config)
        {
            _context = context;
            _hasher = hasher;
            _logger = logger;
            _emailService = emailService;
            _cache = cache;
            _config = config;
        }

        // OTP được lưu theo EMAIL trong bộ nhớ đệm của server (không lưu theo session của người gọi),
        // nên không thể "xin OTP trong session này rồi dò thoải mái" và mỗi mã chỉ có tối đa OtpMaxAttempts lần thử.
        private sealed class OtpEntry
        {
            public string Code { get; init; } = string.Empty;
            public int Attempts { get; set; }
        }

        private static string OtpKey(string email) => "otp:" + email;
        private static string OtpCooldownKey(string email) => "otp-cd:" + email;

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
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);
            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);

            PasswordVerificationResult verify;
            if (user == null)
            {
                // Vẫn băm một lần để thời gian phản hồi không lộ email nào đã tồn tại
                _dummyHash ??= _hasher.HashPassword(new TaiKhoan(), Guid.NewGuid().ToString("N"));
                _hasher.VerifyHashedPassword(new TaiKhoan(), _dummyHash, model.Password);
                verify = PasswordVerificationResult.Failed;
            }
            else
            {
                verify = VerifyPassword(user, model.Password);
            }

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
        [EnableRateLimiting("auth")]
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

        // ===================== QUÊN MẬT KHẨU (FORGOT PASSWORD) =====================

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);

            // Đang trong thời gian chờ gửi lại: bỏ qua im lặng (vẫn chuyển sang bước nhập OTP)
            if (!_cache.TryGetValue(OtpCooldownKey(email), out _))
            {
                // Đặt cooldown cho mọi email (kể cả không tồn tại) để không lộ sự khác biệt
                _cache.Set(OtpCooldownKey(email), true, OtpResendCooldown);

                var user = await _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    // Tạo mã OTP ngẫu nhiên 6 chữ số (100000-999999)
                    var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(CultureInfo.InvariantCulture);
                    _cache.Set(OtpKey(email), new OtpEntry { Code = otp }, OtpLifetime);

                    var mailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                        <h2 style='color: #0056b3;'>Yêu cầu đặt lại mật khẩu - WorkConnect</h2>
                        <p>Xin chào <strong>{WebUtility.HtmlEncode(user.HoTen)}</strong>,</p>
                        <p>Mã OTP để xác minh đặt lại mật khẩu của bạn là:</p>
                        <h1 style='color: #d9534f; letter-spacing: 4px; text-align: center;'>{otp}</h1>
                        <p>Mã này có hiệu lực trong <strong>5 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
                    </div>";

                    // Gửi mail ở nền: thời gian phản hồi và kết quả giống nhau dù email có tồn tại hay không
                    var emailService = _emailService;
                    var logger = _logger;
                    var cache = _cache;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await emailService.SendEmailAsync(email, "Mã OTP đặt lại mật khẩu - WorkConnect", mailBody);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Lỗi gửi email OTP đặt lại mật khẩu cho {Email}", email);
                            // Gửi thất bại: hủy OTP và cooldown để người dùng yêu cầu lại được
                            cache.Remove(OtpKey(email));
                            cache.Remove(OtpCooldownKey(email));
                        }
                    });
                }
            }

            // Luôn chuyển sang bước xác nhận OTP để không lộ email nào tồn tại
            return RedirectToAction(nameof(VerifyOtp), new { email });
        }

        // GET: /Account/VerifyOtp
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(ForgotPassword));

            return View(new VerifyOtpViewModel { Email = email });
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);

            if (_cache.TryGetValue(OtpKey(email), out OtpEntry? entry) && entry != null)
            {
                bool ok;
                bool exhausted;
                lock (entry)
                {
                    if (entry.Attempts >= OtpMaxAttempts)
                    {
                        ok = false;
                        exhausted = true;
                    }
                    else
                    {
                        entry.Attempts++;
                        ok = FixedEquals(entry.Code, model.OtpCode.Trim());
                        exhausted = !ok && entry.Attempts >= OtpMaxAttempts;
                    }
                }

                if (ok)
                {
                    _cache.Remove(OtpKey(email));   // OTP dùng một lần

                    // Token xác thực lượt đổi mật khẩu, gắn với session của người vừa xác minh OTP
                    var resetToken = Guid.NewGuid().ToString("N");
                    HttpContext.Session.SetString("ResetToken_" + email, resetToken);
                    HttpContext.Session.SetString("ResetTokenExpiry_" + email,
                        DateTime.UtcNow.Add(ResetTokenLifetime).ToString("o", CultureInfo.InvariantCulture));

                    return RedirectToAction(nameof(ResetPassword), new { email, token = resetToken });
                }

                if (exhausted)
                    _cache.Remove(OtpKey(email));   // sai quá số lần cho phép -> hủy mã, phải yêu cầu mã mới
            }

            ModelState.AddModelError(nameof(model.OtpCode), "Mã OTP không chính xác hoặc đã hết hạn.");
            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(ForgotPassword));

            if (!IsResetTokenValid(NormalizeEmail(email), token))
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = NormalizeEmail(model.Email);

            if (!IsResetTokenValid(email, model.Token))
            {
                ModelState.AddModelError(string.Empty, "Phiên đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
                return View(model);
            }

            var user = await _context.TaiKhoans.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                // Cập nhật mật khẩu băm mới. Dấu bảo mật đổi theo -> mọi phiên đăng nhập cũ của tài khoản này bị hủy
                user.MatKhauHash = _hasher.HashPassword(user, model.NewPassword);
                await _context.SaveChangesAsync();

                // Dọn dẹp dữ liệu OTP & Token
                _cache.Remove(OtpKey(email));
                HttpContext.Session.Remove("ResetToken_" + email);
                HttpContext.Session.Remove("ResetTokenExpiry_" + email);

                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản cần cập nhật.");
            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
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

        private static bool FixedEquals(string a, string b)
            => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

        /// <summary>Token đặt lại mật khẩu phải khớp (so sánh thời gian cố định) và còn hạn.</summary>
        private bool IsResetTokenValid(string email, string? token)
        {
            var saved = HttpContext.Session.GetString("ResetToken_" + email);
            var expiry = HttpContext.Session.GetString("ResetTokenExpiry_" + email);
            if (saved == null || expiry == null || string.IsNullOrEmpty(token)) return false;

            if (!DateTime.TryParse(expiry, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt)
                || expiresAt <= DateTime.UtcNow)
                return false;

            return FixedEquals(saved, token);
        }

        /// <summary>
        /// Xác thực mật khẩu. Chuỗi băm của PasswordHasher (định dạng v3) luôn bắt đầu bằng "AQAAAA".
        /// Mật khẩu lưu dạng thô (dữ liệu mẫu cũ) chỉ được chấp nhận khi bật Security:AllowLegacyPlaintextPasswords
        /// (mặc định TẮT); nếu khớp thì trả về SuccessRehashNeeded để Login tự băm lại và ghi đè.
        /// </summary>
        private PasswordVerificationResult VerifyPassword(TaiKhoan user, string password)
        {
            var stored = user.MatKhauHash;
            if (string.IsNullOrEmpty(stored))
                return PasswordVerificationResult.Failed;

            if (!stored.StartsWith("AQAAAA", StringComparison.Ordinal))
            {
                if (!_config.GetValue<bool>("Security:AllowLegacyPlaintextPasswords"))
                    return PasswordVerificationResult.Failed;

                return FixedEquals(stored, password)
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
                new(ClaimTypes.Role, AppRoles.FromId(user.MaVaiTro)),
                new(SecurityStamp.ClaimType, SecurityStamp.From(user.MatKhauHash))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var props = new AuthenticationProperties
            {
                IsPersistent = rememberMe, // "Tự động đăng nhập"
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props);
        }
    }
}
