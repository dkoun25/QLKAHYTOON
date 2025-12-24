using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    [AuthorizeUser]
    public class UsersController : BaseController
    {
        private nguoidung CurrentUser => Session["User"] as nguoidung;

        // GET: Users/Profile
        public ActionResult Profile()
        {
            var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);
            return View(userInfo);
        }

        // GET: Users/TruyenTheoDoi
        public ActionResult TruyenTheoDoi()
        {
            var truyenTheoDoi = (from yt in db.truyenyeuthiches
                                 where yt.MaNguoiDung == CurrentUser.MaNguoiDung
                                 join truyen in db.thongtintruyens on yt.MaTruyen equals truyen.MaTruyen
                                 select new
                                 {
                                     truyen,
                                     yt.NgayThem,
                                     ChapterMoi = db.chuongs
                                         .Where(c => c.MaTruyen == truyen.MaTruyen)
                                         .OrderByDescending(c => c.NgayDang)
                                         .FirstOrDefault(),
                                     ChapterDocGanNhat = db.lichsudocs
                                         .Where(l => l.MaNguoiDung == CurrentUser.MaNguoiDung && l.MaTruyen == truyen.MaTruyen)
                                         .OrderByDescending(l => l.ThoiGianDoc)
                                         .Select(l => l.chuong)
                                         .FirstOrDefault()
                                 })
                                .ToList()
                                .Select(x => new TruyenTheoDoiViewModel
                                {
                                    Truyen = x.truyen,
                                    NgayThem = x.NgayThem ?? DateTime.Now,
                                    ChapterMoiNhat = x.ChapterMoi,
                                    ChapterDocGanNhat = x.ChapterDocGanNhat
                                })
                                .OrderByDescending(x => x.ChapterMoiNhat != null ? x.ChapterMoiNhat.NgayDang : (DateTime?)x.NgayThem)
                                .ToList();

            return View(truyenTheoDoi);
        }

        // GET: Users/LichSuDoc
        public ActionResult LichSuDoc()
        {
            var lichSuDoc = (from ls in db.lichsudocs
                             where ls.MaNguoiDung == CurrentUser.MaNguoiDung
                             group ls by ls.MaTruyen into g
                             let lastRead = g.OrderByDescending(x => x.ThoiGianDoc).FirstOrDefault()
                             select new LichSuDocViewModel
                             {
                                 Truyen = lastRead.thongtintruyen,
                                 ChapterDocGanNhat = lastRead.chuong,
                                 ThoiGianDoc = lastRead.ThoiGianDoc ?? DateTime.Now
                             })
                            .OrderByDescending(x => x.ThoiGianDoc)
                            .ToList();

            return View(lichSuDoc);
        }

        // GET: Users/CaiDatTaiKhoan
        public ActionResult CaiDatTaiKhoan()
        {
            var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);
            return View(userInfo);
        }

        // POST: Users/CapNhatThongTin
        [HttpPost]
        public JsonResult CapNhatThongTin(string hoTen, string soDienThoai)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hoTen))
                    return Json(new { success = false, msg = "Họ tên không được để trống!" });

                db.sp_User_CapNhatThongTin(CurrentUser.MaNguoiDung, hoTen.Trim(), null, soDienThoai?.Trim());

                var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);
                Session["User"] = userInfo;

                return Json(new { success = true, msg = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Users/DoiMatKhau
        [HttpPost]
        public JsonResult DoiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            try
            {
                var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);

                if (userInfo.MatKhau != matKhauCu)
                    return Json(new { success = false, msg = "Mật khẩu cũ không đúng!" });

                if (matKhauMoi != xacNhanMatKhau)
                    return Json(new { success = false, msg = "Mật khẩu mới và xác nhận không khớp!" });

                if (matKhauMoi.Length < 6)
                    return Json(new { success = false, msg = "Mật khẩu mới phải có ít nhất 6 ký tự!" });

                db.sp_User_DoiMatKhau(CurrentUser.MaNguoiDung, matKhauMoi);

                return Json(new { success = true, msg = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Users/UploadAvatar
        [HttpPost]
        public JsonResult UploadAvatar(HttpPostedFileBase file)
        {
            try
            {
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, msg = "Vui lòng chọn ảnh!" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return Json(new { success = false, msg = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)!" });

                if (file.ContentLength > 5 * 1024 * 1024)
                    return Json(new { success = false, msg = "Kích thước ảnh không được vượt quá 5MB!" });

                var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);
                var fileName = CurrentUser.MaNguoiDung + "_" + DateTime.Now.Ticks + extension;
                var uploadPath = Server.MapPath("~/Anh/Avatar/");

                if (!System.IO.Directory.Exists(uploadPath))
                    System.IO.Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                file.SaveAs(filePath);

                if (!string.IsNullOrEmpty(userInfo.Avatar) && userInfo.Avatar != "/Anh/Avatar/default-avatar.png")
                {
                    var oldFilePath = Server.MapPath(userInfo.Avatar);
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                var avatarUrl = "/Anh/Avatar/" + fileName;
                db.sp_User_CapNhatThongTin(CurrentUser.MaNguoiDung, userInfo.HoTen, avatarUrl, userInfo.SoDienThoai);

                userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == CurrentUser.MaNguoiDung);
                Session["User"] = userInfo;

                return Json(new { success = true, msg = "Cập nhật ảnh đại diện thành công!", avatarUrl = avatarUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // GET: Users/TruyenYeuThich
        public ActionResult TruyenYeuThich()
        {
            var yeuThich = db.truyenyeuthiches
                             .Where(yt => yt.MaNguoiDung == CurrentUser.MaNguoiDung)
                             .OrderByDescending(yt => yt.NgayThem)
                             .ToList();
            return View(yeuThich);
        }
    }
}