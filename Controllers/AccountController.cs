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

        // POST: Account/Register
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
                    // Tạo một MaNguoiDung ngẫu nhiên hoặc theo quy tắc của bạn
                    MaNguoiDung = "ND" + System.Guid.NewGuid().ToString().Substring(0, 5),
                    HoTen = model.HoTen,
                    Email = model.Email,
                    TenDangNhap = model.TenDangNhap,
                    // MK HIỆN TẠI CHƯA MÃ HÓA NHA HUY 
                    // MatKhau = Crypto.HashPassword(model.MatKhau),
                    MatKhau = model.MatKhau, // Tạm thời chưa mã hóa
                    VaiTro = "user", // Vai trò mặc định là user
                    NgayDangKy = System.DateTime.Now // Sửa thành NgayDangKy
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

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Logic đăng nhập nên so sánh mật khẩu đã được mã hóa
                // var user = db.nguoidungs.SingleOrDefault(u => u.TenDangNhap == model.TenDangNhap);
                // if (user != null && Crypto.VerifyHashedPassword(user.MatKhau, model.MatKhau))
                // { ... }

                // Code tạm thời khi chưa có mã hóa
                var user = db.nguoidungs.SingleOrDefault(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == model.MatKhau);

                if (user != null)
                {
                    Session["User"] = user;
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
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}