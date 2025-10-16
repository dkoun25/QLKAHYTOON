using QLKAHYTOON.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace QLKAHYTOON.Controllers
{
    public class TruyenController : Controller
    {
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

        // GET: Truyen/ChiTiet/MT_1
        // Lưu ý: ID của bạn có thể là string (NCHAR) thay vì int
        public ActionResult ChiTiet(string id)
        {
            // Lấy thông tin truyện dựa vào MaTruyen
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == id);
            if (truyen == null)
            {
                return HttpNotFound(); // Không tìm thấy truyện
            }

            // Lấy danh sách chương của truyện đó, sắp xếp theo SoChuong
            ViewBag.DanhSachChuong = db.chuongs.Where(c => c.MaTruyen == id).OrderBy(c => c.SoChuong).ToList();

            // Lấy danh sách bình luận cho truyện này (tất cả các chương)
            ViewBag.BinhLuan = db.binhluans.Where(b => b.MaTruyen == id).OrderByDescending(b => b.NgayDang).ToList();

            return View(truyen);
        }

        // GET: Truyen/DocTruyen/C001
        public ActionResult DocTruyen(string id)
        {
            // Lấy thông tin chương truyện dựa vào MaChuong
            var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == id);
            if (chuong == null)
            {
                return HttpNotFound();
            }

            // Lấy nội dung hình ảnh từ cột AnhChuong
            if (!string.IsNullOrEmpty(chuong.AnhChuong))
            {
                ViewBag.DanhSachAnh = chuong.AnhChuong.Split(';').ToList();
            }

            // Ghi lại lịch sử đọc nếu người dùng đã đăng nhập
            if (Session["User"] != null)
            {
                var user = Session["User"] as nguoidung;
                // Tạo một MaLichSuDoc ngẫu nhiên hoặc theo quy tắc của bạn
                string maLichSuDoc = "LH" + System.Guid.NewGuid().ToString().Substring(0, 5);

                var lichSu = new lichsudoc
                {
                    MaLichSuDoc = maLichSuDoc,
                    MaNguoiDung = user.MaNguoiDung,
                    MaTruyen = chuong.MaTruyen, // Thêm MaTruyen
                    MaChuong = id,
                    ThoiGianDoc = System.DateTime.Now // Sửa thành ThoiGianDoc
                };
                db.lichsudocs.InsertOnSubmit(lichSu);
                db.SubmitChanges();
            }

            return View(chuong);
        }

        // GET: Truyen/TimKiem?keyword=Naruto
        public ActionResult TimKiem(string keyword)
        {
            var ketQua = db.thongtintruyens.Where(t => t.TenTruyen.Contains(keyword)).ToList();
            ViewBag.Keyword = keyword;
            return View(ketQua);
        }
    }
}