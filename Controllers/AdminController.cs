using Newtonsoft.Json;
using QLKAHYTOON.Helpers;
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
                // Tìm admin theo username
                var adminUser = db.admins.SingleOrDefault(a => a.TenDangNhap == model.TenDangNhap);

                if (adminUser != null)
                {
                    if (PasswordHelper.VerifyPassword(model.MatKhau, adminUser.MatKhau))
                    {
                        Session["Admin"] = adminUser;
                        Session["User"] = adminUser; 
                        Session["IsAdmin"] = true;   
                        return RedirectToAction("Index", "Admin");
                    }
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            return View(model);
        }

        // GET: Admin/Logout
        public ActionResult Logout()
        {
            Session["Admin"] = null;
            Session["User"] = null;   // ⭐ Clear cả Session["User"]
            Session["IsAdmin"] = null; // ⭐ Clear IsAdmin flag
            return RedirectToAction("Login", "Admin");
        }
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
                var tongSoTruyen = db.thongtintruyens.Count();
                var tongNguoiDung = db.nguoidungs.Count();
                var baoCaoChuaXuLy = db.baocaos.Count(b => b.TrangThai == "Chưa xử lý");
                var tongLuotXem = db.lichsudocs.Count();
                var tongLuotTheoDoi = db.truyenyeuthiches.Count();

                var today = DateTime.Today;
                var weeklyViews = new int[7];

                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(-6 + i);
                    var nextDate = date.AddDays(1);
                    weeklyViews[i] = db.lichsudocs.Count(l => l.ThoiGianDoc >= date && l.ThoiGianDoc < nextDate);
                }

                var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var startOfLastWeek = startOfThisWeek.AddDays(-7);

                var thisWeekViews = db.lichsudocs.Count(l => l.ThoiGianDoc >= startOfThisWeek && l.ThoiGianDoc < today.AddDays(1));
                var lastWeekViews = db.lichsudocs.Count(l => l.ThoiGianDoc >= startOfLastWeek && l.ThoiGianDoc < startOfThisWeek);

                var monthlyUsers = new int[30];
                for (int i = 0; i < 30; i++)
                {
                    var date = today.AddDays(-29 + i);
                    var nextDate = date.AddDays(1);
                    monthlyUsers[i] = db.nguoidungs.Count(n => n.NgayDangKy >= date && n.NgayDangKy < nextDate);
                }

                ViewBag.TongSoTruyen = tongSoTruyen;
                ViewBag.TongNguoiDung = tongNguoiDung;
                ViewBag.BaoCaoChuaXuLy = baoCaoChuaXuLy;
                ViewBag.TongLuotXem = tongLuotXem;
                ViewBag.TongLuotTheoDoi = tongLuotTheoDoi;
                ViewBag.WeeklyViewsJson = JsonConvert.SerializeObject(weeklyViews);
                ViewBag.LastWeekViewsJson = lastWeekViews;
                ViewBag.ThisWeekViewsJson = thisWeekViews;
                ViewBag.MonthlyUsersJson = JsonConvert.SerializeObject(monthlyUsers);

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi tải dữ liệu: " + ex.Message;
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
        public ActionResult QuanLyTruyen(int? page)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var allTruyen = db.thongtintruyens.GroupBy(t => t.MaTruyen).Select(g => g.FirstOrDefault()).OrderByDescending(t => t.NgayDang).ToList();
            int tongSoTruyen = allTruyen.Count();
            int totalPages = (int)Math.Ceiling((double)tongSoTruyen / pageSize);
            var danhSachTruyen = allTruyen.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var allTheLoai = db.theloais.ToList();
            var truyenTheLoaiDict = new Dictionary<string, List<string>>();
            foreach (var truyen in danhSachTruyen)
            {
                var danhSachTenTheLoai = new List<string>();
                if (!string.IsNullOrEmpty(truyen.MaTheLoai))
                {
                    var cacMaTheLoai = truyen.MaTheLoai.Split(',');
                    foreach (var ma in cacMaTheLoai)
                    {
                        var maClean = ma.Trim();
                        var theLoaiObj = allTheLoai.FirstOrDefault(x => x.MaTheLoai.Trim() == maClean);
                        if (theLoaiObj != null) danhSachTenTheLoai.Add(theLoaiObj.TenTheLoai.Trim());
                    }
                }
                truyenTheLoaiDict[truyen.MaTruyen.Trim()] = danhSachTenTheLoai;
            }
            ViewBag.TruyenTheLoaiDict = truyenTheLoaiDict;
            ViewBag.TongSoTruyen = tongSoTruyen;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            return View(danhSachTruyen);
        }

        // AJAX: Lấy danh sách truyện theo trang
        [HttpGet]
        public JsonResult GetTruyenByPage(int page = 1, int pageSize = 8)
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" }, JsonRequestBehavior.AllowGet);
            try
            {
                int skip = (page - 1) * pageSize;
                var danhSachTruyen = db.thongtintruyens.GroupBy(t => t.MaTruyen).Select(g => g.FirstOrDefault()).OrderByDescending(t => t.NgayDang).Skip(skip).Take(pageSize).ToList();
                var allTheLoai = db.theloais.ToList();
                var result = danhSachTruyen.Select(t => {
                    var danhSachTenTheLoai = new List<string>();
                    if (!string.IsNullOrEmpty(t.MaTheLoai))
                    {
                        var cacMaTheLoai = t.MaTheLoai.Split(',');
                        foreach (var ma in cacMaTheLoai)
                        {
                            var maClean = ma.Trim();
                            var theLoaiObj = allTheLoai.FirstOrDefault(x => x.MaTheLoai.Trim() == maClean);
                            if (theLoaiObj != null) danhSachTenTheLoai.Add(theLoaiObj.TenTheLoai.Trim());
                        }
                    }
                    return new { maTruyen = t.MaTruyen.Trim(), tenTruyen = t.TenTruyen.Trim(), tacGia = t.TacGia.Trim(), anhTruyen = t.AnhTruyen, trangThai = t.TrangThai.Trim(), theLoai = danhSachTenTheLoai };
                }).ToList();
                int tongSoTruyen = db.thongtintruyens.GroupBy(t => t.MaTruyen).Count();
                int tongSoTrang = (int)Math.Ceiling((double)tongSoTruyen / pageSize);
                return Json(new { success = true, data = result, currentPage = page, totalPages = tongSoTrang, totalItems = tongSoTruyen }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
        // GET: Admin/ThemTruyen
        public ActionResult ThemTruyen()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            ViewBag.ListTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
            int maxId = 0;
            var listMaTruyen = db.thongtintruyens.Select(t => t.MaTruyen).ToList();
            foreach (var ma in listMaTruyen)
            {
                string phanSo = ma.Replace("MT_", "").Trim();
                int so;
                if (int.TryParse(phanSo, out so)) { if (so > maxId) maxId = so; }
            }
            string maTruyenMoi = "MT_" + (maxId + 1);
            ViewBag.MaTruyenAuto = maTruyenMoi;
            ViewBag.IsEdit = false;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemTruyen(thongtintruyen model, HttpPostedFileBase fileAnh, string[] theloai)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if (fileAnh == null || fileAnh.ContentLength == 0) ModelState.AddModelError("", "Vui lòng chọn ảnh bìa truyện.");
            if (theloai == null || theloai.Length == 0) ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thể loại.");
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = Path.GetFileName(fileAnh.FileName);
                    string newFileName = "cover_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
                    string path = Path.Combine(Server.MapPath("~/Anh/BiaTruyen/"), newFileName);
                    if (!Directory.Exists(Server.MapPath("~/Anh/BiaTruyen/"))) Directory.CreateDirectory(Server.MapPath("~/Anh/BiaTruyen/"));
                    fileAnh.SaveAs(path);
                    string relativePath = "/Anh/BiaTruyen/" + newFileName;
                    string strMaTheLoai = string.Join(", ", theloai);
                    string maTruyenMoi = model.MaTruyen;
                    string moTaTruyen = model.Slug ?? "";
                    int ketQua = db.sp_Admin_ThemTruyen(maTruyenMoi, strMaTheLoai, model.TenTruyen, model.TacGia, model.TrangThai, relativePath, moTaTruyen);
                    if (ketQua == 1) { TempData["UploadSuccess"] = "Thêm truyện mới thành công!"; return RedirectToAction("QuanLyTruyen"); }
                    else ModelState.AddModelError("", "Có lỗi khi lưu vào CSDL, có thể trùng mã truyện");
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); }
            }
            ViewBag.ListTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
            ViewBag.IsEdit = false;
            return View(model);
        }

        // GET: Admin/SuaTruyen
        public ActionResult SuaTruyen(string maTruyen)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if (string.IsNullOrEmpty(maTruyen)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var truyen = db.thongtintruyens.FirstOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null) return HttpNotFound();
            ViewBag.ListTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
            ViewBag.IsEdit = true;
            ViewBag.MaTruyenAuto = truyen.MaTruyen;
            var selectedGenres = new List<string>();
            if (!string.IsNullOrEmpty(truyen.MaTheLoai)) selectedGenres = truyen.MaTheLoai.Split(',').Select(m => m.Trim()).ToList();
            ViewBag.SelectedGenres = selectedGenres;
            return View("ThemTruyen", truyen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SuaTruyen(thongtintruyen model, HttpPostedFileBase fileAnh, string[] theloai)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if (theloai == null || theloai.Length == 0) ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 thể loại.");
            if (ModelState.IsValid)
            {
                try
                {
                    var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == model.MaTruyen);
                    if (truyen == null) return HttpNotFound();
                    if (fileAnh != null && fileAnh.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(fileAnh.FileName);
                        string newFileName = "cover_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(fileName);
                        string path = Path.Combine(Server.MapPath("~/Anh/BiaTruyen/"), newFileName);
                        if (!Directory.Exists(Server.MapPath("~/Anh/BiaTruyen/"))) Directory.CreateDirectory(Server.MapPath("~/Anh/BiaTruyen/"));
                        fileAnh.SaveAs(path);
                        truyen.AnhTruyen = "/Anh/BiaTruyen/" + newFileName;
                    }
                    truyen.TenTruyen = model.TenTruyen;
                    truyen.TacGia = model.TacGia;
                    truyen.TrangThai = model.TrangThai;
                    truyen.Slug = model.Slug ?? "";
                    truyen.MaTheLoai = string.Join(", ", theloai.Select(t => t.Trim()));
                    db.SubmitChanges();
                    TempData["UploadSuccess"] = "Cập nhật truyện thành công!";
                    return RedirectToAction("QuanLyTruyen");
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); if (ex.InnerException != null) ModelState.AddModelError("", "Chi tiết: " + ex.InnerException.Message); }
            }
            ViewBag.ListTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
            ViewBag.IsEdit = true;
            var selectedGenres = theloai != null ? theloai.ToList() : new List<string>();
            ViewBag.SelectedGenres = selectedGenres;
            return View("ThemTruyen", model);
        }

        // GET: Admin/XoaTruyen
        public ActionResult XoaTruyen(string maTruyen)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            try
            {
                var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
                if (truyen == null) { TempData["Error"] = "Không tìm thấy truyện!"; return RedirectToAction("QuanLyTruyen"); }
                var theLoais = db.thongtintruyens.Where(ttl => ttl.MaTruyen == maTruyen);
                db.thongtintruyens.DeleteAllOnSubmit(theLoais);
                var yeuThichs = db.truyenyeuthiches.Where(yt => yt.MaTruyen == maTruyen);
                db.truyenyeuthiches.DeleteAllOnSubmit(yeuThichs);
                var lichSus = db.lichsudocs.Where(ls => ls.MaTruyen == maTruyen);
                db.lichsudocs.DeleteAllOnSubmit(lichSus);
                var chuongs = db.chuongs.Where(c => c.MaTruyen == maTruyen).Select(c => c.MaChuong).ToList();
                foreach (var maChuong in chuongs) { var binhLuans = db.binhluans.Where(bl => bl.MaChuong == maChuong); db.binhluans.DeleteAllOnSubmit(binhLuans); }
                var baoCaos = db.baocaos.Where(bc => bc.MaTruyen == maTruyen);
                db.baocaos.DeleteAllOnSubmit(baoCaos);
                var allChuongs = db.chuongs.Where(c => c.MaTruyen == maTruyen);
                db.chuongs.DeleteAllOnSubmit(allChuongs);
                db.thongtintruyens.DeleteOnSubmit(truyen);
                db.SubmitChanges();
                TempData["UploadSuccess"] = "Xóa truyện thành công!";
            }
            catch (Exception ex) { TempData["Error"] = "Có lỗi xảy ra: " + ex.Message; }
            return RedirectToAction("QuanLyTruyen");
        }

        // GET: Admin/ThemChuong
        public ActionResult ThemChuong(string maTruyen)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if (string.IsNullOrEmpty(maTruyen)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null) return HttpNotFound();

            // Tính số chương tiếp theo
            var maxSoChuong = db.chuongs
                .Where(c => c.MaTruyen == maTruyen)
                .OrderByDescending(c => c.SoChuong)
                .Select(c => c.SoChuong)
                .FirstOrDefault();

            int soChuongMoi = (maxSoChuong ?? 0) + 1;

            // Lấy danh sách chương đã có
            var danhSachChuong = db.chuongs
                .Where(c => c.MaTruyen == maTruyen)
                .OrderBy(c => c.SoChuong)
                .ToList();

            ViewBag.ThongTinTruyen = truyen;
            ViewBag.SoChuongMoi = soChuongMoi;
            ViewBag.DanhSachChuong = danhSachChuong;
            return View();
        }

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

        // AJAX: Lấy thông tin chương để sửa
        [HttpGet]
        public JsonResult GetChapterInfo(string maChuong)
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" }, JsonRequestBehavior.AllowGet);

            try
            {
                var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == maChuong);
                if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương" }, JsonRequestBehavior.AllowGet);

                var anhChuongs = !string.IsNullOrEmpty(chuong.AnhChuong)
                    ? chuong.AnhChuong.Split(';').ToList()
                    : new List<string>();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        maChuong = chuong.MaChuong,
                        soChuong = chuong.SoChuong,
                        tenChuong = chuong.TenChuong,
                        anhChuongs = anhChuongs
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Cập nhật thông tin chương
        [HttpPost]
        public JsonResult UpdateChapter(string maChuong, string tenChuong, string anhChuongJson)
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" });

            try
            {
                var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == maChuong);
                if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương" });

                chuong.TenChuong = tenChuong;

                // Cập nhật thứ tự ảnh nếu có
                if (!string.IsNullOrEmpty(anhChuongJson))
                {
                    var anhList = JsonConvert.DeserializeObject<List<string>>(anhChuongJson);
                    chuong.AnhChuong = string.Join(";", anhList);
                }

                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật chương thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // AJAX: Xóa ảnh trong chương
        [HttpPost]
        public JsonResult DeleteChapterImage(string maChuong, string imagePath)
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" });

            try
            {
                var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == maChuong);
                if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương" });

                var anhList = chuong.AnhChuong.Split(';').ToList();
                anhList.Remove(imagePath);
                chuong.AnhChuong = string.Join(";", anhList);

                // Xóa file vật lý
                string physicalPath = Server.MapPath("~" + imagePath);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                db.SubmitChanges();

                return Json(new { success = true, message = "Đã xóa ảnh!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // AJAX: Xóa chương
        [HttpPost]
        public JsonResult DeleteChapter(string maChuong)
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" });

            try
            {
                var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == maChuong);
                if (chuong == null) return Json(new { success = false, message = "Không tìm thấy chương" });

                // Xóa thư mục chứa ảnh
                var truyen = db.thongtintruyens.Single(t => t.MaTruyen == chuong.MaTruyen);
                string truyenSlug = SlugHelper.GenerateSlug(truyen.TenTruyen);
                string chapterPath = Server.MapPath("~/Anh/" + truyenSlug + "/" + chuong.SoChuong);

                if (Directory.Exists(chapterPath))
                {
                    Directory.Delete(chapterPath, true);
                }

                // Xóa bình luận của chương
                var binhLuans = db.binhluans.Where(bl => bl.MaChuong == maChuong);
                db.binhluans.DeleteAllOnSubmit(binhLuans);

                // Xóa chương
                db.chuongs.DeleteOnSubmit(chuong);
                db.SubmitChanges();

                return Json(new { success = true, message = "Đã xóa chương thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult QuanLyNguoiDung(int? page)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var allUsers = db.nguoidungs.Where(u => u.VaiTro != "Admin").OrderByDescending(u => u.NgayDangKy).ToList();
            int tongSoNguoiDung = allUsers.Count();
            int totalPages = (int)Math.Ceiling((double)tongSoNguoiDung / pageSize);
            var listUser = allUsers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TongSoNguoiDung = tongSoNguoiDung;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            return View(listUser);
        }

        public ActionResult ToggleTrangThaiUser(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            var user = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == id);
            if (user != null)
            {
                if (user.VaiTro == "Blocked") { user.VaiTro = "User"; TempData["Success"] = "Đã mở khóa tài khoản " + user.TenDangNhap; }
                else { db.sp_Admin_KhoaTaiKhoan(id); TempData["Success"] = "Đã khóa tài khoản " + user.TenDangNhap + ". Tài khoản này không thể bình luận."; }
                db.SubmitChanges();
            }
            return RedirectToAction("QuanLyNguoiDung");
        }
        // GET: Admin/QuanLyBaoCao
        public ActionResult QuanLyBaoCao(int? page)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var loadOptions = new System.Data.Linq.DataLoadOptions();
            loadOptions.LoadWith<baocao>(b => b.nguoidung);
            loadOptions.LoadWith<baocao>(b => b.admin);
            loadOptions.LoadWith<baocao>(b => b.thongtintruyen);
            loadOptions.LoadWith<baocao>(b => b.chuong);
            db.LoadOptions = loadOptions;
            var allBaoCao = db.baocaos.OrderBy(b => b.TrangThai).ThenByDescending(b => b.NgayBaoCao).ToList();
            int tongSoBaoCao = allBaoCao.Count();
            int totalPages = (int)Math.Ceiling((double)tongSoBaoCao / pageSize);
            var listBaoCao = allBaoCao.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TongSoBaoCao = tongSoBaoCao;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            return View(listBaoCao);
        }

        public ActionResult DuyetBaoCao(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            var currentAdmin = Session["Admin"] as admin;
            string maAdmin = currentAdmin.MaAdmin;
            db.sp_Admin_DuyetBaoCao(id, maAdmin);
            db.SubmitChanges();
            TempData["Success"] = "Đã xử lý xong báo cáo!";
            return RedirectToAction("QuanLyBaoCao");
        }


        // Khóa tài khoản từ báo cáo
        public ActionResult KhoaTaiKhoanTuBaoCao(string maNguoiDung, string maBaoCao)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            try
            {
                var user = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == maNguoiDung);
                if (user != null)
                {
                    user.VaiTro = "Blocked";
                    var currentAdmin = Session["Admin"] as admin;
                    db.sp_Admin_DuyetBaoCao(maBaoCao, currentAdmin.MaAdmin);
                    db.SubmitChanges();
                    TempData["Success"] = "Đã khóa tài khoản " + user.TenDangNhap + ". Tài khoản này không thể bình luận.";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Có lỗi xảy ra: " + ex.Message; }
            return RedirectToAction("QuanLyBaoCao");
        }

        public ActionResult QuanLyAdmin()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            return View(db.admins.ToList());
        }

        [HttpPost]
        public ActionResult ThemAdminMoi(string tenDangNhap, string matKhau, string hoTen, string email)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");

            // ⭐ HASH MẬT KHẨU TRƯỚC KHI LƯU
            string hashedPassword = PasswordHelper.HashPassword(matKhau);
            string maAdmin = "AD" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();

            // ⭐ LƯU MẬT KHẨU ĐÃ HASH
            int kq = db.sp_Admin_ThemAdmin(maAdmin, tenDangNhap, hashedPassword, hoTen, email);

            if (kq == 1) TempData["Success"] = "Thêm Admin thành công!";
            else TempData["Error"] = "Tên đăng nhập đã tồn tại!";
            return RedirectToAction("QuanLyAdmin");
        }

        public ActionResult SuaAdmin(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            var admin = db.admins.SingleOrDefault(a => a.MaAdmin == id);
            if (admin == null) return HttpNotFound();
            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaAdmin(admin model)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            if (ModelState.IsValid)
            {
                try
                {
                    var admin = db.admins.SingleOrDefault(a => a.MaAdmin == model.MaAdmin);
                    if (admin != null)
                    {
                        admin.TenDangNhap = model.TenDangNhap;
                        admin.HoTen = model.HoTen;
                        admin.Email = model.Email;

                        // ⭐ CHỈ CẬP NHẬT MẬT KHẨU NẾU CÓ NHẬP MẬT KHẨU MỚI VÀ HASH NÓ
                        if (!string.IsNullOrEmpty(model.MatKhau))
                        {
                            admin.MatKhau = PasswordHelper.HashPassword(model.MatKhau);
                        }

                        db.SubmitChanges();
                        TempData["Success"] = "Cập nhật Admin thành công!";
                        return RedirectToAction("QuanLyAdmin");
                    }
                }
                catch (Exception ex) { ModelState.AddModelError("", "Lỗi: " + ex.Message); }
            }
            return View(model);
        }

        public ActionResult XoaAdmin(string id)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "Admin");
            var currentAdmin = Session["Admin"] as admin;
            if (currentAdmin.MaAdmin == id) { TempData["Error"] = "Bạn không thể tự xóa chính mình!"; return RedirectToAction("QuanLyAdmin"); }
            db.sp_Admin_XoaAdmin(id);
            TempData["Success"] = "Đã xóa Admin.";
            return RedirectToAction("QuanLyAdmin");
        }
        [HttpGet]
        public JsonResult GetDashboardData()
        {
            if (!IsAdminLoggedIn()) return Json(new { success = false, message = "Chưa đăng nhập" }, JsonRequestBehavior.AllowGet);
            try
            {
                var today = DateTime.Today;
                var weeklyViews = new int[7];
                for (int i = 0; i < 7; i++)
                {
                    var date = today.AddDays(-6 + i);
                    var nextDate = date.AddDays(1);
                    weeklyViews[i] = db.lichsudocs.Count(l => l.ThoiGianDoc >= date && l.ThoiGianDoc < nextDate);
                }
                var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var startOfLastWeek = startOfThisWeek.AddDays(-7);
                var thisWeekViews = db.lichsudocs.Count(l => l.ThoiGianDoc >= startOfThisWeek && l.ThoiGianDoc < today.AddDays(1));
                var lastWeekViews = db.lichsudocs.Count(l => l.ThoiGianDoc >= startOfLastWeek && l.ThoiGianDoc < startOfThisWeek);
                var monthlyUsers = new int[30];
                for (int i = 0; i < 30; i++)
                {
                    var date = today.AddDays(-29 + i);
                    var nextDate = date.AddDays(1);
                    monthlyUsers[i] = db.nguoidungs.Count(n => n.NgayDangKy >= date && n.NgayDangKy < nextDate);
                }
                return Json(new { success = true, data = new { tongSoTruyen = db.thongtintruyens.Count(), tongNguoiDung = db.nguoidungs.Count(), baoCaoChuaXuLy = db.baocaos.Count(b => b.TrangThai == "Chưa xử lý"), tongLuotXem = db.lichsudocs.Count(), tongLuotTheoDoi = db.truyenyeuthiches.Count(), weeklyViews = weeklyViews, thisWeekViews = thisWeekViews, lastWeekViews = lastWeekViews, monthlyUsers = monthlyUsers } }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet); }
        }
    }

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