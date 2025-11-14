using QLKAHYTOON.Models;
using System.Linq;
using System.Web.Mvc;
using System; // Cần cho Guid, DateTime
using System.Collections.Generic; // Cần cho List

namespace QLKAHYTOON.Controllers
{
    public class TruyenController : Controller
    {
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext();

        // GET: Truyen/ChiTiet/MT_1
        public ActionResult ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // 1. Lấy thông tin truyện và JOIN với thể loại để lấy tên
            var truyen = (from t in db.thongtintruyens
                          join tl in db.theloais on t.MaTheLoai equals tl.MaTheLoai
                          where t.MaTruyen == id
                          select new
                          {
                              TruyenInfo = t,
                              TenTheLoai = tl.TenTheLoai
                          }).SingleOrDefault();

            if (truyen == null)
            {
                return HttpNotFound();
            }

            // 2. Lấy danh sách chương (truyền qua ViewBag)
            ViewBag.DanhSachChuong = db.chuongs.Where(c => c.MaTruyen == id).OrderBy(c => c.SoChuong).ToList();

            // 3. Lấy danh sách bình luận (JOIN với nguoidung để lấy Tên và Avatar)
            var danhSachBinhLuan = (from bl in db.binhluans
                                    join nd in db.nguoidungs on bl.MaNguoiDung equals nd.MaNguoiDung
                                    where bl.MaTruyen == id
                                    orderby bl.NgayDang descending
                                    select new
                                    {
                                        bl.NoiDung,
                                        bl.NgayDang,
                                        nd.HoTen, // Lấy họ tên từ bảng nguoidung
                                        nd.Avatar // Lấy luôn avatar (nếu có)
                                    }).ToList();

            ViewBag.BinhLuan = danhSachBinhLuan;

            // 4. Truyền Tên Thể Loại ra View
            ViewBag.TenTheLoai = truyen.TenTheLoai;

            // 5. Trả về đối tượng thongtintruyen làm Model chính
            return View(truyen.TruyenInfo);
        }

        // GET: Truyen/DocTruyen/C001
        public ActionResult DocTruyen(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == id);
            if (chuong == null)
            {
                return HttpNotFound();
            }

            // Tách chuỗi ảnh thành một danh sách List<string>
            if (!string.IsNullOrEmpty(chuong.AnhChuong))
            {
                // Chia chuỗi (được lưu bằng dấu ";") thành một danh sách
                ViewBag.DanhSachAnh = chuong.AnhChuong.Split(';').ToList();
            }
            else
            {
                ViewBag.DanhSachAnh = new List<string>(); // Trả về danh sách rỗng
            }

            // Ghi lại lịch sử đọc nếu người dùng đã đăng nhập
            if (Session["User"] != null)
            {
                var user = Session["User"] as nguoidung;

                var lichSu = new lichsudoc
                {
                    MaLichSuDoc = "LH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaNguoiDung = user.MaNguoiDung,
                    MaTruyen = chuong.MaTruyen,
                    MaChuong = id,
                    ThoiGianDoc = DateTime.Now // Cột quan trọng cho Top Ranking
                };
                db.lichsudocs.InsertOnSubmit(lichSu);
                db.SubmitChanges();
            }

            // Trả về đối tượng chuong (Model) để View sử dụng
            return View(chuong);
        }

        // GET: Truyen/TimKiem?keyword=Naruto
        public ActionResult TimKiem(string keyword)
        {
            var ketQua = db.thongtintruyens
                           .Where(t => t.TenTruyen.Contains(keyword))
                           .ToList();

            ViewBag.Keyword = keyword;
            return View(ketQua);
        }
    }
}