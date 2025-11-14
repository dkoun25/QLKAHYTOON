using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO; // Cần cho việc xử lý file/thư mục
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class AdminController : Controller
    {
        // KHỞI TẠO DATACONTEXT     
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(
            System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString
        );

        // GET: Admin/Login
        public ActionResult Login()
        {
            return View();
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

        // GET: Admin/Logout
        public ActionResult Logout()
        {
            Session["Admin"] = null; // Chỉ xóa session của Admin
            return RedirectToAction("Login", "Admin");
        }


        // --- DASHBOARD VÀ CÁC CHỨC NĂNG KHÁC ---

        // Hàm kiểm tra session (để dùng nội bộ)
        private bool IsAdminLoggedIn()
        {
            return Session["Admin"] != null;
        }

        // GET: Admin (Dashboard)
        public ActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            // Lấy dữ liệu thống kê
            ViewBag.SoLuongTruyen = db.thongtintruyens.Count();
            ViewBag.SoLuongNguoiDung = db.nguoidungs.Count();
            ViewBag.SoLuongBaoCao = db.baocaos.Count(b => b.TrangThai == "Chưa xử lý");

            return View();
        }

        // GET: Admin/QuanLyTruyen
        public ActionResult QuanLyTruyen()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            var danhSachTruyen = db.thongtintruyens.OrderByDescending(t => t.NgayDang).ToList();
            return View(danhSachTruyen);
        }

        // GET: Admin/QuanLyNguoiDung
        public ActionResult QuanLyNguoiDung()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            var danhSachNguoiDung = db.nguoidungs.ToList();
            return View(danhSachNguoiDung);
        }

        // --- CHỨC NĂNG THÊM CHƯƠNG ---

        // GET: Admin/ThemChuong?maTruyen=MT_1
        public ActionResult ThemChuong(string maTruyen)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            if (string.IsNullOrEmpty(maTruyen))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null)
            {
                return HttpNotFound();
            }

            // Truyền thông tin truyện sang View
            ViewBag.ThongTinTruyen = truyen;
            return View();
        }

        // POST: Admin/ThemChuong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemChuong(string maTruyen, int soChuong, string tenChuong, IEnumerable<HttpPostedFileBase> files)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            if (files == null || !files.Any() || files.First() == null)
            {
                TempData["UploadError"] = "Vui lòng chọn ít nhất một file ảnh.";
                return RedirectToAction("ThemChuong", new { maTruyen = maTruyen });
            }

            var truyen = db.thongtintruyens.Single(t => t.MaTruyen == maTruyen);

            // Dùng SlugHelper phiên bản GỐC (không bỏ dấu)
            string truyenSlug = SlugHelper.GenerateSlug(truyen.TenTruyen);
            string chapterPath = Server.MapPath("~/Anh/" + truyenSlug + "/" + soChuong);

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

                    string relativePath = "/Anh/" + truyenSlug + "/" + soChuong + "/" + fileName;
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

    // Lớp SlugHelper GỐC (Không bỏ dấu Tiếng Việt)
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            // Lọc ký tự đặc biệt
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Thay khoảng trắng bằng gạch nối
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}