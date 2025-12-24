using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using QLKAHYTOON.Helpers;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register - Với mã hóa mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên đăng nhập đã tồn tại
                if (db.nguoidungs.Any(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại
                if (db.nguoidungs.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email đã được sử dụng.");
                    return View(model);
                }

                try
                {
                    // MÃ HÓA MẬT KHẨU
                    string hashedPassword = PasswordHelper.HashPassword(model.MatKhau);

                    var newUser = new nguoidung
                    {
                        MaNguoiDung = "ND" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                        HoTen = model.HoTen,
                        Email = model.Email,
                        TenDangNhap = model.TenDangNhap,
                        MatKhau = hashedPassword,
                        VaiTro = "user",
                        NgayDangKy = DateTime.Now
                    };

                    db.nguoidungs.InsertOnSubmit(newUser);
                    db.SubmitChanges();

                    TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }
            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login - FIXED: Admin login được nhận diện đầy đủ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm user theo tên đăng nhập
                var user = db.nguoidungs.SingleOrDefault(u => u.TenDangNhap == model.TenDangNhap);

                if (user != null)
                {
                    // Xác thực mật khẩu đã hash
                    if (PasswordHelper.VerifyPassword(model.MatKhau, user.MatKhau))
                    {
                        Session["User"] = user;
                        Session["IsAdmin"] = false; // User thường
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Nếu không phải user, kiểm tra admin
                var admin = db.admins.SingleOrDefault(a => a.TenDangNhap == model.TenDangNhap);

                if (admin != null)
                {
                    // Xác thực mật khẩu admin
                    if (PasswordHelper.VerifyPassword(model.MatKhau, admin.MatKhau))
                    {
                        // ⭐ FIX: Set Session["User"] = admin để admin được nhận diện
                        Session["User"] = admin;
                        Session["IsAdmin"] = true;
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
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