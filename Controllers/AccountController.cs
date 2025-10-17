using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class AccountController : Controller
    {
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register (Chỉ đăng ký NGUOIDUNG)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.nguoidungs.Any(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
                if (db.nguoidungs.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View(model);
                }

                var newUser = new nguoidung
                {
                    MaNguoiDung = "ND" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                    HoTen = model.HoTen,
                    Email = model.Email,
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = model.MatKhau, // !!! NHỚ MÃ HÓA MẬT KHẨU !!!
                    VaiTro = "user",
                    NgayDangKy = System.DateTime.Now
                };

                db.nguoidungs.InsertOnSubmit(newUser);
                db.SubmitChanges();

                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login (Chỉ đăng nhập NGUOIDUNG)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tạm thời chưa mã hóa
                var user = db.nguoidungs.SingleOrDefault(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

                if (user != null)
                {
                    Session["User"] = user; // Lưu session cho NguoiDung
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa tất cả session (cả User và Admin)
            return RedirectToAction("Index", "Home");
        }
    }
}