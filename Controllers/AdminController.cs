using QLKAHYTOON.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    // Giả sử bạn có cơ chế check quyền Admin, nếu chưa có thì tạm thời bỏ qua
    // [AuthorizeUser(Roles = "admin")] 
    public class AdminController : Controller
    {
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

        // GET: Admin (Dashboard)
        public ActionResult Index()
        {
            return View();
        }

        // --- CÁC ACTION MỚI CHO SIDEBAR ---

        public ActionResult QuanLyTruyen()
        {
            // Logic để hiển thị danh sách truyện sẽ được thêm vào đây
            ViewBag.Message = "Đây là trang quản lý truyện.";
            return View(); // Bạn sẽ cần tạo View cho Action này
        }

        public ActionResult QuanLyNguoiDung()
        {
            ViewBag.Message = "Đây là trang quản lý người dùng.";
            return View();
        }

        public ActionResult QuanLyBaoCao()
        {
            ViewBag.Message = "Đây là trang quản lý báo cáo.";
            return View();
        }

        public ActionResult QuanLyAdmin()
        {
            ViewBag.Message = "Đây là trang quản lý các admin khác.";
            return View();
        }

        public ActionResult CaiDat()
        {
            ViewBag.Message = "Đây là trang cài đặt tài khoản của bạn.";
            return View();
        }
        // GET: Admin/ThemChuong?maTruyen=MT_1
        public ActionResult ThemChuong(string maTruyen)
        {
            if (string.IsNullOrEmpty(maTruyen))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null)
            {
                return HttpNotFound();
            }

            // Đưa thông tin truyện ra View để biết đang thêm chương cho truyện nào
            ViewBag.ThongTinTruyen = truyen;
            return View();
        }

        // POST: Admin/ThemChuong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemChuong(string maTruyen, int soChuong, string tenChuong, IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any() || files.First() == null)
            {
                // Xử lý lỗi nếu không có file nào được chọn
                TempData["UploadError"] = "Vui lòng chọn ít nhất một file ảnh.";
                return RedirectToAction("ThemChuong", new { maTruyen = maTruyen });
            }

            // 1. Tạo đường dẫn thư mục để lưu ảnh
            // Ví dụ: ~/Uploads/Chapters/pham-nhan-tu-tien/1/
            var truyen = db.thongtintruyens.Single(t => t.MaTruyen == maTruyen);
            string truyenSlug = SlugHelper.GenerateSlug(truyen.TenTruyen); // Hàm tạo slug (tên-truyen-khong-dau)
            string chapterPath = Server.MapPath("~/Uploads/Chapters/" + truyenSlug + "/" + soChuong);

            // Tạo thư mục nếu nó chưa tồn tại
            Directory.CreateDirectory(chapterPath);

            var imagePaths = new List<string>();
            int imageCounter = 1;

            // 2. Lặp qua từng file, lưu và tạo đường dẫn tương đối
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Đặt lại tên file để tránh trùng lặp và dễ sắp xếp, vd: 01.jpg, 02.jpg
                    string extension = Path.GetExtension(file.FileName);
                    string fileName = imageCounter.ToString("D2") + extension; // D2 -> 01, 02, ...
                    string savedFilePath = Path.Combine(chapterPath, fileName);

                    file.SaveAs(savedFilePath);

                    // 3. Tạo đường dẫn tương đối để lưu vào CSDL
                    string relativePath = "/Uploads/Chapters/" + truyenSlug + "/" + soChuong + "/" + fileName;
                    imagePaths.Add(relativePath);
                    imageCounter++;
                }
            }

            // 4. Chuẩn bị dữ liệu và lưu chương mới vào CSDL
            var newChapter = new chuong
            {
                MaChuong = "C" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                MaTruyen = maTruyen,
                SoChuong = soChuong,
                TenChuong = tenChuong,
                AnhChuong = string.Join(";", imagePaths), // Nối các đường dẫn lại
                NgayDang = DateTime.Now
            };

            db.chuongs.InsertOnSubmit(newChapter);
            db.SubmitChanges();

            TempData["UploadSuccess"] = $"Đã thêm thành công chương {soChuong} cho truyện {truyen.TenTruyen}!";
            return RedirectToAction("ThemChuong", new { maTruyen = maTruyen });
        }
    }

    // LỚP HỖ TRỢ: Tạo một file mới tên SlugHelper.cs hoặc đặt ở cuối file controller
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            // Xóa các ký tự đặc biệt, thay khoảng trắng bằng gạch ngang
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}