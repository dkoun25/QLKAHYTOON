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
            // Lấy tất cả bình luận của truyện này (bao gồm cả các chap)
            // Chỉ lấy bình luận GỐC (MaBinhLuanCha là null)
            var listBinhLuan = db.binhluans
                .Where(x => x.MaTruyen == maTruyen && x.MaBinhLuanCha == null)
                .OrderByDescending(x => x.NgayDang)
                .ToList();

            ViewBag.IsDetail = isDetail;
            ViewBag.MaTruyen = maTruyen;

            return PartialView("BinhLuanTruyen", listBinhLuan);
        }

        // 2. THÊM BÌNH LUẬN (AJAX)
        [HttpPost]
        public ActionResult PostBinhLuan(string maTruyen, string maChuong, string noiDung, string maBinhLuanCha)
        {
            var user = Session["User"] as nguoidung; // Lấy từ Session đăng nhập
            if (user == null)
            {
                return Json(new { success = false, msg = "Bạn cần đăng nhập để bình luận!" });
            }

            if (string.IsNullOrEmpty(noiDung))
            {
                return Json(new { success = false, msg = "Nội dung không được để trống." });
            }

            try
            {
                var bl = new binhluan();
                bl.MaBinhLuan = Guid.NewGuid().ToString(); // Tạo ID ngẫu nhiên vì DB là nvarchar
                bl.MaTruyen = maTruyen;

                // Nếu maChuong rỗng (bình luận ở trang chi tiết) thì gán null
                bl.MaChuong = string.IsNullOrEmpty(maChuong) ? null : maChuong;

                bl.MaNguoiDung = user.MaNguoiDung;
                bl.NoiDung = noiDung;
                bl.NgayDang = DateTime.Now;
                bl.MaBinhLuanCha = string.IsNullOrEmpty(maBinhLuanCha) ? null : maBinhLuanCha;
                bl.LuotLike = 0;

                db.binhluans.InsertOnSubmit(bl);
                db.SubmitChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Ghi log ex.Message nếu cần
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // 3. LIKE BÌNH LUẬN
        [HttpPost]
        public ActionResult LikeBinhLuan(string maBinhLuan)
        {
            var bl = db.binhluans.SingleOrDefault(x => x.MaBinhLuan==maBinhLuan);
            if (bl != null)
            {
                bl.LuotLike = (bl.LuotLike ?? 0) + 1;
                db.SubmitChanges();
                return Json(new { success = true, likes = bl.LuotLike });
            }
            return Json(new { success = false });
        }
    }
}