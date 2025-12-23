using QLKAHYTOON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class BinhLuanController : BaseController
    {
        // GET: BinhLuan
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListBinhLuan(string maTruyen, bool isDetail)
        {
            // Lấy tất cả bình luận của truyện này (1 lần query)
            var allComments = db.binhluans
                .Where(x => x.MaTruyen == maTruyen)
                .ToList();

            // Lọc bình luận GỐC (MaBinhLuanCha là null)
            var listBinhLuan = allComments
                .Where(x => x.MaBinhLuanCha == null)
                .OrderByDescending(x => x.NgayDang)
                .ToList();

            // Load replies cho mỗi comment và lưu vào ViewData
            foreach (var comment in allComments)
            {
                var replies = allComments
                    .Where(x => x.MaBinhLuanCha == comment.MaBinhLuan)
                    .OrderBy(x => x.NgayDang)
                    .ToList();

                ViewData["Replies_" + comment.MaBinhLuan] = replies;
            }

            ViewBag.IsDetail = isDetail;
            ViewBag.MaTruyen = maTruyen;

            return PartialView("BinhLuanTruyen", listBinhLuan);
        }

        // 2. THÊM BÌNH LUẬN (AJAX) - CẬP NHẬT KIỂM TRA REAL-TIME
        [HttpPost]
        public ActionResult PostBinhLuan(string maTruyen, string maChuong, string noiDung, string maBinhLuanCha)
        {
            var user = Session["User"] as nguoidung;

            // Kiểm tra đăng nhập
            if (user == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập để bình luận!" });
            }

            // KIỂM TRA TRẠNG THÁI TÀI KHOẢN REAL-TIME TỪ DATABASE
            var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

            if (userFromDb == null)
            {
                // Xóa session nếu user không tồn tại
                Session["User"] = null;
                return Json(new
                {
                    success = false,
                    msg = "Tài khoản không tồn tại!",
                    needReload = true // Báo cho client reload trang
                });
            }

            // Kiểm tra trạng thái khóa từ database
            if (userFromDb.VaiTro == "Blocked")
            {
                // Cập nhật session
                Session["User"] = userFromDb;

                return Json(new
                {
                    success = false,
                    msg = "Tài khoản của bạn đã bị khóa. Bạn không thể bình luận!",
                    isBlocked = true
                });
            }

            // Cập nhật session với dữ liệu mới nhất từ DB
            Session["User"] = userFromDb;

            // Kiểm tra nội dung
            if (string.IsNullOrEmpty(noiDung) || string.IsNullOrWhiteSpace(noiDung))
            {
                return Json(new { success = false, msg = "Nội dung không được để trống." });
            }

            try
            {
                var bl = new binhluan();
                bl.MaBinhLuan = Guid.NewGuid().ToString();
                bl.MaTruyen = maTruyen;
                bl.MaChuong = string.IsNullOrEmpty(maChuong) ? null : maChuong;
                bl.MaNguoiDung = userFromDb.MaNguoiDung;
                bl.NoiDung = noiDung.Trim();
                bl.NgayDang = DateTime.Now;
                bl.MaBinhLuanCha = string.IsNullOrEmpty(maBinhLuanCha) ? null : maBinhLuanCha;
                bl.LuotLike = 0;

                db.binhluans.InsertOnSubmit(bl);
                db.SubmitChanges();

                return Json(new { success = true, msg = "Bình luận thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // 3. LIKE BÌNH LUẬN - CẬP NHẬT KIỂM TRA REAL-TIME
        [HttpPost]
        public ActionResult LikeBinhLuan(string maBinhLuan)
        {
            var user = Session["User"] as nguoidung;

            // Kiểm tra đăng nhập
            if (user == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập để thích bình luận!" });
            }

            // KIỂM TRA TRẠNG THÁI TÀI KHOẢN REAL-TIME
            var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

            if (userFromDb == null)
            {
                Session["User"] = null;
                return Json(new
                {
                    success = false,
                    msg = "Tài khoản không tồn tại!",
                    needReload = true
                });
            }

            // Kiểm tra tài khoản bị khóa
            if (userFromDb.VaiTro == "Blocked")
            {
                Session["User"] = userFromDb;
                return Json(new
                {
                    success = false,
                    msg = "Tài khoản của bạn đã bị khóa!",
                    isBlocked = true
                });
            }

            // Cập nhật session
            Session["User"] = userFromDb;

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

        // 4. XÓA BÌNH LUẬN - CẬP NHẬT KIỂM TRA REAL-TIME
        [HttpPost]
        public ActionResult XoaBinhLuan(string maBinhLuan)
        {
            var user = Session["User"] as nguoidung;

            if (user == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập!" });
            }

            // KIỂM TRA TRẠNG THÁI TÀI KHOẢN REAL-TIME
            var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);

            if (userFromDb == null)
            {
                Session["User"] = null;
                return Json(new
                {
                    success = false,
                    msg = "Tài khoản không tồn tại!",
                    needReload = true
                });
            }

            if (userFromDb.VaiTro == "Blocked")
            {
                Session["User"] = userFromDb;
                return Json(new
                {
                    success = false,
                    msg = "Tài khoản của bạn đã bị khóa!",
                    isBlocked = true
                });
            }

            // Cập nhật session
            Session["User"] = userFromDb;

            try
            {
                var bl = db.binhluans.SingleOrDefault(x => x.MaBinhLuan == maBinhLuan);

                if (bl == null)
                {
                    return Json(new { success = false, msg = "Không tìm thấy bình luận!" });
                }

                // Kiểm tra quyền: chỉ cho phép xóa bình luận của chính mình
                if (bl.MaNguoiDung != userFromDb.MaNguoiDung)
                {
                    return Json(new { success = false, msg = "Bạn không có quyền xóa bình luận này!" });
                }

                // Xóa tất cả replies của bình luận này trước (recursive)
                XoaRepliesRecursive(maBinhLuan);

                // Xóa bình luận chính
                db.binhluans.DeleteOnSubmit(bl);
                db.SubmitChanges();

                return Json(new { success = true, msg = "Đã xóa bình luận!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // Helper method: Xóa replies đệ quy
        private void XoaRepliesRecursive(string maBinhLuanCha)
        {
            var replies = db.binhluans.Where(x => x.MaBinhLuanCha == maBinhLuanCha).ToList();

            foreach (var reply in replies)
            {
                // Xóa replies của reply này trước (đệ quy)
                XoaRepliesRecursive(reply.MaBinhLuan);

                // Xóa reply
                db.binhluans.DeleteOnSubmit(reply);
            }
        }
    }
}