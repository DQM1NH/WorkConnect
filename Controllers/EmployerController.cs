
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkConnect.Data;

namespace WorkConnect.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employers = await _context.NhaTuyenDungs
                .AsNoTracking()
                .ToListAsync();

            return View(employers);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var employer = await _context.NhaTuyenDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaNhaTuyenDung == id);

            if (employer == null)
            {
                return NotFound();
            }

            return View(employer);
        }
        
        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.TinTuyenDungs
                .AsNoTracking()
                .OrderByDescending(x => x.NgayDang)
                .ToListAsync();

            return View(jobs);
        }


    }
}
