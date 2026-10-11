using Microsoft.EntityFrameworkCore;
using WorkConnect.Data;
using WorkConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC (chỉ đăng ký một lần)
builder.Services.AddControllersWithViews();

// Kết nối CSDL SQL Server (chuỗi kết nối nằm trong appsettings.json hoặc User Secrets)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tầng Service (Controller -> Service -> DbContext)
builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();