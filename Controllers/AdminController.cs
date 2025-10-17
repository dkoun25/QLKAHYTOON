using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class AdminController : Controller
    {
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

        // --- PHẦN ĐĂNG NHẬP ADMIN MỚI ---

        // GET: Admin/Login
        public ActionResult Login()
        {
            return View(); // Chúng ta sẽ tạo 1 View Login riêng cho Admin
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm kiếm trong bảng ADMIN
                var adminUser = db.admins.SingleOrDefault(a => a.TenDangNhap == model.TenDangNhap && a.MatKhau == model.MatKhau);

                if (adminUser != null)
                {
                    // Lưu session riêng cho Admin
                    Session["Admin"] = adminUser;
                    return RedirectToAction("Index", "Admin"); // Chuyển đến Dashboard
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản Admin không đúng.");
                }
            }
            return View(model);
        }

        // --- DASHBOARD VÀ CÁC CHỨC NĂNG KHÁC ---

        // GET: Admin (Dashboard)
        public ActionResult Index()
        {
            // Kiểm tra xem Admin đã đăng nhập chưa
            if (Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            // (Code logic cho dashboard... ví dụ: đếm số truyện, số người dùng)
            return View();
        }

        // --- CÁC ACTION MỚI CHO SIDEBAR ---
        public ActionResult QuanLyTruyen()
        {
            if (Session["Admin"] == null) return RedirectToAction("Login", "Admin");

            var danhSachTruyen = db.thongtintruyens.ToList();
            return View(danhSachTruyen);
        }

        public ActionResult QuanLyNguoiDung()
        {
            if (Session["Admin"] == null) return RedirectToAction("Login", "Admin");

            var danhSachNguoiDung = db.nguoidungs.ToList();
            return View(danhSachNguoiDung);
        }

        // (Thêm các Action khác cho QuanLyBaoCao, QuanLyAdmin, CaiDat...)

        // --- CHỨC NĂNG THÊM CHƯƠNG ---

        // GET: Admin/ThemChuong?maTruyen=MT_1
        public ActionResult ThemChuong(string maTruyen)
        {
            if (Session["Admin"] == null) return RedirectToAction("Login", "Admin");

            if (string.IsNullOrEmpty(maTruyen))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null)
            {
                return HttpNotFound();
            }

            ViewBag.ThongTinTruyen = truyen;
            return View();
        }

        // POST: Admin/ThemChuong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemChuong(string maTruyen, int soChuong, string tenChuong, IEnumerable<HttpPostedFileBase> files)
        {
            if (Session["Admin"] == null) return RedirectToAction("Login", "Admin");

            if (files == null || !files.Any() || files.First() == null)
            {
                TempData["UploadError"] = "Vui lòng chọn ít nhất một file ảnh.";
                return RedirectToAction("ThemChuong", new { maTruyen = maTruyen });
            }

            var truyen = db.thongtintruyens.Single(t => t.MaTruyen == maTruyen);
            string truyenSlug = SlugHelper.GenerateSlug(truyen.TenTruyen);
            string chapterPath = Server.MapPath("~/Uploads/Chapters/" + truyenSlug + "/" + soChuong);

            Directory.CreateDirectory(chapterPath);

            var imagePaths = new List<string>();
            int imageCounter = 1;

            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string extension = Path.GetExtension(file.FileName);
                    string fileName = imageCounter.ToString("D2") + extension;
                    string savedFilePath = Path.Combine(chapterPath, fileName);

                    file.SaveAs(savedFilePath);

                    string relativePath = "/Uploads/Chapters/" + truyenSlug + "/" + soChuong + "/" + fileName;
                    imagePaths.Add(relativePath);
                    imageCounter++;
                }
            }

            var newChapter = new chuong
            {
                MaChuong = "C" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                MaTruyen = maTruyen,
                SoChuong = soChuong,
                TenChuong = tenChuong,
                AnhChuong = string.Join(";", imagePaths),
                NgayDang = DateTime.Now
            };

            db.chuongs.InsertOnSubmit(newChapter);
            db.SubmitChanges();

            TempData["UploadSuccess"] = $"Đã thêm thành công chương {soChuong}!";
            return RedirectToAction("ThemChuong", new { maTruyen = maTruyen });
        }
    }

    // Lớp SlugHelper giữ nguyên
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}