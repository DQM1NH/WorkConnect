using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Models;
using WorkConnect.Models.Entities;
using WorkConnect.Services;
using WorkConnect.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Email Service
builder.Services.AddTransient<IEmailService, EmailService>();

// Cấu hình DbContext
builder.Services.AddDbContext<WorkConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// 1. DỊCH VỤ SESSION (Thêm bộ nhớ đệm và cấu hình Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15); // Thời gian Session hết hạn
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Băm mật khẩu (PBKDF2 + salt ngẫu nhiên)
builder.Services.AddScoped<IPasswordHasher<TaiKhoan>, PasswordHasher<TaiKhoan>>();

// Đăng nhập bằng cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);   // phiên thường
        options.SlidingExpiration = true;
        options.Cookie.Name = "WorkConnect.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        // Mỗi request kiểm tra lại: tài khoản bị khóa hoặc bị xóa thì hủy phiên đăng nhập ngay
        options.Events.OnValidatePrincipal = async context =>
        {
            var idClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<WorkConnectDbContext>();
            var valid = await db.TaiKhoans.AsNoTracking().AnyAsync(u => u.MaTaiKhoan == userId && !u.BiKhoa);
            if (!valid)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DemoDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<TaiKhoan>>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 2. MIDDLEWARE SESSION (Đặt giữa UseRouting và UseAuthentication)
app.UseSession();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();