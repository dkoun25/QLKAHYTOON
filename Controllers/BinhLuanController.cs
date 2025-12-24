using QLKAHYTOON.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class BinhLuanController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        // ⭐ FIX: Pass ViewBag đầy đủ để Partial View nhận được
        public ActionResult ListBinhLuan(string maTruyen, bool isDetail)
        {
            var allComments = db.binhluans
                .Where(x => x.MaTruyen == maTruyen)
                .ToList();

            var listBinhLuan = allComments
                .Where(x => x.MaBinhLuanCha == null)
                .OrderByDescending(x => x.NgayDang)
                .ToList();

            foreach (var comment in allComments)
            {
                var replies = allComments
                    .Where(x => x.MaBinhLuanCha == comment.MaBinhLuan)
                    .OrderBy(x => x.NgayDang)
                    .ToList();

                ViewData["Replies_" + comment.MaBinhLuan] = replies;
            }

            // ⭐ FIX: Pass thông tin user/admin qua ViewBag
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            ViewBag.User = user;
            ViewBag.Admin = admin;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsDetail = isDetail;
            ViewBag.MaTruyen = maTruyen;

            return PartialView("BinhLuanTruyen", listBinhLuan);
        }

        // POST: Thêm bình luận - Hỗ trợ Admin
        [HttpPost]
        public ActionResult PostBinhLuan(string maTruyen, string maChuong, string noiDung, string maBinhLuanCha)
        {
            // Kiểm tra đăng nhập (User hoặc Admin)
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            if (user == null && admin == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập để bình luận!" });
            }

            string maNguoiDung = null;
            string hoTen = null;
            string avatar = null;

            if (isAdmin && admin != null)
            {
                // Admin đang bình luận
                maNguoiDung = admin.MaAdmin;
                hoTen = admin.HoTen;
                avatar = "/Anh/admin-avatar.png";
            }
            else if (user != null)
            {
                // User bình thường
                var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

                if (userFromDb == null)
                {
                    Session["User"] = null;
                    return Json(new { success = false, msg = "Tài khoản không tồn tại!", needReload = true });
                }

                if (userFromDb.VaiTro == "Blocked")
                {
                    Session["User"] = userFromDb;
                    return Json(new { success = false, msg = "Tài khoản của bạn đã bị khóa!", isBlocked = true });
                }

                Session["User"] = userFromDb;
                maNguoiDung = userFromDb.MaNguoiDung;
                hoTen = userFromDb.HoTen;
                avatar = userFromDb.Avatar;
            }

            if (string.IsNullOrEmpty(noiDung) || string.IsNullOrWhiteSpace(noiDung))
            {
                return Json(new { success = false, msg = "Nội dung không được để trống." });
            }

            try
            {
                var bl = new binhluan
                {
                    MaBinhLuan = Guid.NewGuid().ToString(),
                    MaTruyen = maTruyen,
                    MaChuong = string.IsNullOrEmpty(maChuong) ? null : maChuong,
                    MaNguoiDung = maNguoiDung,
                    NoiDung = noiDung.Trim(),
                    NgayDang = DateTime.Now,
                    MaBinhLuanCha = string.IsNullOrEmpty(maBinhLuanCha) ? null : maBinhLuanCha,
                    LuotLike = 0
                };

                db.binhluans.InsertOnSubmit(bl);
                db.SubmitChanges();

                return Json(new { success = true, msg = "Bình luận thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult LikeBinhLuan(string maBinhLuan)
        {
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;

            if (user == null && admin == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập để thích bình luận!" });
            }

            if (user != null)
            {
                var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

                if (userFromDb == null)
                {
                    Session["User"] = null;
                    return Json(new { success = false, msg = "Tài khoản không tồn tại!", needReload = true });
                }

                if (userFromDb.VaiTro == "Blocked")
                {
                    Session["User"] = userFromDb;
                    return Json(new { success = false, msg = "Tài khoản của bạn đã bị khóa!", isBlocked = true });
                }

                Session["User"] = userFromDb;
            }

            try
            {
                var bl = db.binhluans.SingleOrDefault(x => x.MaBinhLuan == maBinhLuan);
                if (bl != null)
                {
                    bl.LuotLike = (bl.LuotLike ?? 0) + 1;
                    db.SubmitChanges();
                    return Json(new { success = true, likes = bl.LuotLike });
                }
                return Json(new { success = false, msg = "Không tìm thấy bình luận!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult XoaBinhLuan(string maBinhLuan)
        {
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            if (user == null && admin == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập!" });
            }

            if (!isAdmin && user != null)
            {
                var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

                if (userFromDb == null)
                {
                    Session["User"] = null;
                    return Json(new { success = false, msg = "Tài khoản không tồn tại!", needReload = true });
                }

                if (userFromDb.VaiTro == "Blocked")
                {
                    Session["User"] = userFromDb;
                    return Json(new { success = false, msg = "Tài khoản của bạn đã bị khóa!", isBlocked = true });
                }

                Session["User"] = userFromDb;
            }

            try
            {
                var bl = db.binhluans.SingleOrDefault(x => x.MaBinhLuan == maBinhLuan);

                if (bl == null)
                {
                    return Json(new { success = false, msg = "Không tìm thấy bình luận!" });
                }

                bool canDelete = false;
                if (isAdmin && admin != null)
                {
                    canDelete = true;
                }
                else if (user != null && bl.MaNguoiDung == user.MaNguoiDung)
                {
                    canDelete = true;
                }

                if (!canDelete)
                {
                    return Json(new { success = false, msg = "Bạn không có quyền xóa bình luận này!" });
                }

                XoaRepliesRecursive(maBinhLuan);
                db.binhluans.DeleteOnSubmit(bl);
                db.SubmitChanges();

                return Json(new { success = true, msg = "Đã xóa bình luận!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        private void XoaRepliesRecursive(string maBinhLuanCha)
        {
            var replies = db.binhluans.Where(x => x.MaBinhLuanCha == maBinhLuanCha).ToList();

            foreach (var reply in replies)
            {
                XoaRepliesRecursive(reply.MaBinhLuan);
                db.binhluans.DeleteOnSubmit(reply);
            }
        }
    }
}