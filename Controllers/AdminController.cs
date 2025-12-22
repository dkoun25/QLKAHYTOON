using Newtonsoft.Json;
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
    public class AdminController : BaseController
    {

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

            try
            {
                // 1. Tổng số truyện
                var tongSoTruyen = db.thongtintruyens.Count();

                // 2. Tổng số người dùng
                var tongNguoiDung = db.nguoidungs.Count();

                // 3. Báo cáo chưa xử lý
                var baoCaoChuaXuLy = db.baocaos.Count(b => b.TrangThai == "Chưa xử lý");

                // 4. Tổng lượt xem (từ bảng lichsudoc)
                var tongLuotXem = db.lichsudocs.Count();

                // 5. Tổng lượt theo dõi (từ bảng theodoi)
                var tongLuotTheoDoi = db.truyenyeuthiches.Count();

                // 6. Dữ liệu lượt xem 7 ngày qua (cho biểu đồ đường)
                var today = DateTime.Today;
                var weeklyViews = new int[7];

                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(-6 + i);
                    var nextDate = date.AddDays(1);

                    weeklyViews[i] = db.lichsudocs
                        .Count(l => l.ThoiGianDoc >= date && l.ThoiGianDoc < nextDate);
                }

                // 7. So sánh tuần này với tuần trước
                var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Thứ 2 tuần này
                var startOfLastWeek = startOfThisWeek.AddDays(-7);

                var thisWeekViews = db.lichsudocs
                    .Count(l => l.ThoiGianDoc >= startOfThisWeek && l.ThoiGianDoc < today.AddDays(1));

                var lastWeekViews = db.lichsudocs
                    .Count(l => l.ThoiGianDoc >= startOfLastWeek && l.ThoiGianDoc < startOfThisWeek);

                // 8. Số người dùng mới trong 30 ngày
                var monthlyUsers = new int[30];

                for (int i = 0; i < 30; i++)
                {
                    var date = today.AddDays(-29 + i);
                    var nextDate = date.AddDays(1);

                    monthlyUsers[i] = db.nguoidungs
                        .Count(n => n.NgayDangKy >= date && n.NgayDangKy < nextDate);
                }

                // Gửi dữ liệu sang View
                ViewBag.TongSoTruyen = tongSoTruyen;
                ViewBag.TongNguoiDung = tongNguoiDung;
                ViewBag.BaoCaoChuaXuLy = baoCaoChuaXuLy;
                ViewBag.TongLuotXem = tongLuotXem;
                ViewBag.TongLuotTheoDoi = tongLuotTheoDoi;

                // Chuyển dữ liệu sang JSON cho Chart.js
                ViewBag.WeeklyViewsJson = JsonConvert.SerializeObject(weeklyViews);
                ViewBag.LastWeekViewsJson = lastWeekViews;
                ViewBag.ThisWeekViewsJson = thisWeekViews;
                ViewBag.MonthlyUsersJson = JsonConvert.SerializeObject(monthlyUsers);

                // Giữ lại code cũ để tương thích
                ViewBag.SoLuongTruyen = tongSoTruyen;
                ViewBag.SoLuongNguoiDung = tongNguoiDung;
                ViewBag.SoLuongBaoCao = baoCaoChuaXuLy;

                return View();
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                ViewBag.Error = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;

                // Giá trị mặc định nếu lỗi
                ViewBag.TongSoTruyen = 0;
                ViewBag.TongNguoiDung = 0;
                ViewBag.BaoCaoChuaXuLy = 0;
                ViewBag.TongLuotXem = 0;
                ViewBag.TongLuotTheoDoi = 0;
                ViewBag.WeeklyViewsJson = "[0,0,0,0,0,0,0]";
                ViewBag.LastWeekViewsJson = 0;
                ViewBag.ThisWeekViewsJson = 0;
                ViewBag.MonthlyUsersJson = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]";

                return View();
            }
        }
        
        // GET: Admin/QuanLyTruyen
        public ActionResult QuanLyTruyen()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            var danhSachTruyen = db.thongtintruyens.OrderByDescending(t => t.NgayDang).ToList();
            return View(danhSachTruyen);
        }

        // Thêm truyện
        public ActionResult ThemTruyen()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            ViewBag.ListTheLoai=db.theloais.OrderBy(tl=>tl.TenTheLoai).ToList();
            int maxId = 0;

            // Lấy tất cả mã truyện trong database
            var listMaTruyen = db.thongtintruyens.Select(t => t.MaTruyen).ToList();

            foreach (var ma in listMaTruyen)
            {
                // Giả sử mã có dạng "MT_12", ta thay thế "MT_" bằng rỗng để lấy số "12"
                string phanSo = ma.Replace("MT_", "").Trim();
                int so;

                // Thử ép kiểu sang số nguyên (đề phòng có mã lạ không phải số)
                if (int.TryParse(phanSo, out so))
                {
                    if (so > maxId) maxId = so;
                }
            }

            // Mã mới = Số lớn nhất + 1
            string maTruyenMoi = "MT_" + (maxId + 1);

            // Truyền sang View
            ViewBag.MaTruyenAuto = maTruyenMoi;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemTruyen(thongtintruyen model, HttpPostedFileBase fileAnh, string[] theloai)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if(fileAnh == null || fileAnh.ContentLength == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ảnh bìa truyện.");
            }
            if(theloai==null || theloai.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thể loại.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = Path.GetFileName(fileAnh.FileName);
                    string newFileName = "cover_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
                    string path = Path.Combine(Server.MapPath("~/Anh/BiaTruyen/"), newFileName);

                    //tao thu muc bia truyen
                    if (!Directory.Exists(Server.MapPath("~/Anh/BiaTruyen/")))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Anh/BiaTruyen/"));
                    }
                    fileAnh.SaveAs(path);
                    string relativePath = "/Anh/BiaTruyen/" + newFileName;

                    string strMaTheLoai = string.Join(", ", theloai);

                    string maTruyenMoi = model.MaTruyen;
                    string moTaTruyen = model.Slug;
                    if (string.IsNullOrEmpty(moTaTruyen)) moTaTruyen = "";
                    int ketQua = db.sp_Admin_ThemTruyen(
                         maTruyenMoi, // Truyền mã MT_19 vào đây
                         strMaTheLoai,
                         model.TenTruyen,
                         model.TacGia,
                         model.TrangThai,
                         relativePath,
                         moTaTruyen
                        );
                    if (ketQua == 1)
                    {
                        TempData["UploadSuccess"] = "Thêm truyện mới thành công!";
                        return RedirectToAction("QuanLyTruyen");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Có lỗi khi lưu vào CSDL, có thể trùng mã truyện");
                    }
                }
                catch (Exception ex) 
                {
                    ModelState.AddModelError("","Lỗi: " +ex.Message);
                }
            }
            ViewBag.ListTheLoai=db.theloais.OrderBy(tl=>tl.TenTheLoai).ToList();
            return View(model);
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

        public ActionResult QuanLyNguoiDung()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            // Lấy danh sách User (Trừ Admin ra nếu bảng nguoidung có cả admin)
            var listUser = db.nguoidungs.Where(u => u.VaiTro != "Admin").ToList();
            return View(listUser);
        }

        // Khóa / Mở khóa tài khoản
        public ActionResult ToggleTrangThaiUser(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            var user = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == id);
            if (user != null)
            {
                // Logic: Nếu đang Blocked thì mở, ngược lại thì khóa
                if (user.VaiTro == "Blocked")
                {
                    user.VaiTro = "User"; // Mở khóa
                    TempData["Success"] = "Đã mở khóa tài khoản " + user.TenDangNhap;
                }
                else
                {
                    // Gọi SP khóa hoặc sửa trực tiếp
                    db.sp_Admin_KhoaTaiKhoan(id);
                    // Hoặc: user.VaiTro = "Blocked";
                    TempData["Success"] = "Đã khóa tài khoản " + user.TenDangNhap;
                }
                db.SubmitChanges();
            }
            return RedirectToAction("QuanLyNguoiDung");
        }

        // --- QUẢN LÝ BÁO CÁO ---
        public ActionResult QuanLyBaoCao()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            // Lấy báo cáo chưa xử lý lên đầu
            var listBaoCao = db.baocaos.OrderBy(b => b.TrangThai).ThenByDescending(b => b.NgayBaoCao).ToList();
            return View(listBaoCao);
        }

        // Duyệt báo cáo (Đã xử lý xong)
        public ActionResult DuyetBaoCao(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            // Lấy mã Admin đang đăng nhập
            var currentAdmin = Session["AdminUser"] as admin;
            string maAdmin = currentAdmin.MaAdmin;

            db.sp_Admin_DuyetBaoCao(id, maAdmin);
            db.SubmitChanges();

            TempData["Success"] = "Đã xử lý xong báo cáo!";
            return RedirectToAction("QuanLyBaoCao");
        }

        // --- QUẢN LÝ ADMIN ---
        public ActionResult QuanLyAdmin()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            return View(db.admins.ToList());
        }

        [HttpPost]
        public ActionResult ThemAdminMoi(string tenDangNhap, string matKhau, string hoTen, string email)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            string maAdmin = "AD" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // Gọi SP vừa tạo ở Phần 1
            int kq = db.sp_Admin_ThemAdmin(maAdmin, tenDangNhap, matKhau, hoTen, email);

            if (kq == 1) TempData["Success"] = "Thêm Admin thành công!";
            else TempData["Error"] = "Tên đăng nhập đã tồn tại!";

            return RedirectToAction("QuanLyAdmin");
        }

        // Xóa Admin
        public ActionResult XoaAdmin(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            // Không cho tự xóa chính mình
            var currentAdmin = Session["AdminUser"] as admin;
            if (currentAdmin.MaAdmin == id)
            {
                TempData["Error"] = "Bạn không thể tự xóa chính mình!";
                return RedirectToAction("QuanLyAdmin");
            }

            db.sp_Admin_XoaAdmin(id);
            TempData["Success"] = "Đã xóa Admin.";
            return RedirectToAction("QuanLyAdmin");
        }

        [HttpGet]
        public JsonResult GetDashboardData()
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Chưa đăng nhập" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var today = DateTime.Today;

                // Lượt xem 7 ngày qua
                var weeklyViews = new int[7];
                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(-6 + i);
                    var nextDate = date.AddDays(1);
                    weeklyViews[i] = db.lichsudocs
                        .Count(l => l.ThoiGianDoc >= date && l.ThoiGianDoc < nextDate);
                }

                // So sánh tuần
                var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var startOfLastWeek = startOfThisWeek.AddDays(-7);

                var thisWeekViews = db.lichsudocs
                    .Count(l => l.ThoiGianDoc >= startOfThisWeek && l.ThoiGianDoc < today.AddDays(1));

                var lastWeekViews = db.lichsudocs
                    .Count(l => l.ThoiGianDoc >= startOfLastWeek && l.ThoiGianDoc < startOfThisWeek);

                // Người dùng mới 30 ngày
                var monthlyUsers = new int[30];
                for (int i = 0; i < 30; i++)
                {
                    var date = today.AddDays(-29 + i);
                    var nextDate = date.AddDays(1);
                    monthlyUsers[i] = db.nguoidungs
                        .Count(n => n.NgayDangKy >= date && n.NgayDangKy < nextDate);
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        tongSoTruyen = db.thongtintruyens.Count(),
                        tongNguoiDung = db.nguoidungs.Count(),
                        baoCaoChuaXuLy = db.baocaos.Count(b => b.TrangThai == "Chưa xử lý"),
                        tongLuotXem = db.lichsudocs.Count(),
                        tongLuotTheoDoi = db.truyenyeuthiches.Count(),
                        weeklyViews = weeklyViews,
                        thisWeekViews = thisWeekViews,
                        lastWeekViews = lastWeekViews,
                        monthlyUsers = monthlyUsers
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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