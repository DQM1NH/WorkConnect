using Microsoft.AspNetCore.Mvc;
using WorkConnect.Models;

namespace WorkConnect.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Xử lý xác thực đăng nhập
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Xử lý lưu tài khoản mới
                return RedirectToAction("Login");
            }
            return View(model);
        }
    }
}