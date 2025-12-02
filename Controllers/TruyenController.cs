using QLKAHYTOON.Models;
using System.Linq;
using System.Web.Mvc;
using System; // Cần cho Guid, DateTime
using System.Collections.Generic; // Cần cho List

namespace QLKAHYTOON.Controllers
{
    public class TruyenController : BaseController
    {
        // GET: Truyen/ChiTiet/MT_1
        public ActionResult ChiTiet(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // 1. Lấy thông tin truyện (Không JOIN nữa, lấy trực tiếp)
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == id);

            if (truyen == null)
            {
                return HttpNotFound();
            }

            // --- XỬ LÝ NHIỀU THỂ LOẠI ---
            var listTheLoai = new List<theloai>();
            if (!string.IsNullOrEmpty(truyen.MaTheLoai))
            {
                // Tách chuỗi "TL01, TL02" thành mảng ID
                var maTheLoaiArray = truyen.MaTheLoai.Split(',').Select(m => m.Trim()).ToList();

                // Tìm các thể loại có mã nằm trong danh sách trên
                listTheLoai = db.theloais.Where(tl => maTheLoaiArray.Contains(tl.MaTheLoai)).ToList();
            }
            // Truyền danh sách đối tượng thể loại sang View
            ViewBag.DanhSachTheLoai = listTheLoai;


            ViewBag.DanhSachChuong = db.chuongs.Where(c => c.MaTruyen == id).OrderBy(c => c.SoChuong).ToList();

            var danhSachBinhLuan = (from bl in db.binhluans
                                    join nd in db.nguoidungs on bl.MaNguoiDung equals nd.MaNguoiDung
                                    where bl.MaTruyen == id
                                    orderby bl.NgayDang descending
                                    select new
                                    {
                                        bl.NoiDung,
                                        bl.NgayDang,
                                        nd.HoTen,
                                        nd.Avatar
                                    }).ToList();

            ViewBag.BinhLuan = danhSachBinhLuan;

            return View(truyen);
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

            string maNguoiDung = "KHACH"; // Mặc định là khách

            if (Session["User"] != null)
            {
                var user = Session["User"] as nguoidung;
                maNguoiDung = user.MaNguoiDung;
            }

            // Ghi nhận lượt xem (cho cả Khách và User)
            var lichSu = new lichsudoc
            {
                MaLichSuDoc = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaNguoiDung = maNguoiDung, // Dùng mã lấy được ở trên
                MaTruyen = chuong.MaTruyen,
                MaChuong = id,
                ThoiGianDoc = DateTime.Now
            };

            db.lichsudocs.InsertOnSubmit(lichSu);
            db.SubmitChanges();

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