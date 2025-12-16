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
            if (!string.IsNullOrEmpty(chuong.AnhChuong))
            {
                ViewBag.DanhSachAnh = chuong.AnhChuong.Split(';').ToList();
            }
            else
            {
                ViewBag.DanhSachAnh = new List<string>();
            }
            // --- Logic tìm chương trước/sau VÀ Lấy danh sách chương ---
            var cacChuong = db.chuongs.Where(c => c.MaTruyen == chuong.MaTruyen)
                                      .OrderBy(c => c.SoChuong)
                                      .ToList();

            // Gửi danh sách này sang View để làm Modal chọn chương
            ViewBag.ListChuong = cacChuong;

            int index = cacChuong.FindIndex(c => c.MaChuong == id);

            if (index > 0) ViewBag.MaChuongTruoc = cacChuong[index - 1].MaChuong.Trim();
            else ViewBag.MaChuongTruoc = null;

            if (index < cacChuong.Count - 1) ViewBag.MaChuongSau = cacChuong[index + 1].MaChuong.Trim();
            else ViewBag.MaChuongSau = null;
            // ---------------------------------------------

            string maNguoiDung = "KHACH";
            if (Session["User"] != null)
            {
                var user = Session["User"] as nguoidung;
                maNguoiDung = user.MaNguoiDung;
            }
            var lichSu = new lichsudoc
            {
                MaLichSuDoc = "LS" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                MaNguoiDung = maNguoiDung,
                MaTruyen = chuong.MaTruyen,
                MaChuong = id,
                ThoiGianDoc = DateTime.Now
            };
            db.lichsudocs.InsertOnSubmit(lichSu);
            db.SubmitChanges();

            return View(chuong);
        }

        // GET: Truyen/TimKiem?keyword=Naruto
        [HttpGet]
        public ActionResult TimKiem(string keyword, int? page)
        {
            // Nếu từ khóa rỗng -> về trang chủ
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TuKhoa = keyword;
            // --- QUAN TRỌNG: Lấy danh sách tất cả Thể loại để View hiển thị tên thể loại ---
            var allTheLoai = db.theloais.ToList();
            ViewBag.AllTheLoai = allTheLoai;

            // --- LOGIC TÌM KIẾM KHÔNG DẤU ---
            // B1: Lấy hết danh sách truyện (hoặc lọc sơ bộ để tối ưu nếu dữ liệu quá lớn)
            var allTruyen = db.thongtintruyens.ToList();

            // B2: Chuẩn hóa từ khóa tìm kiếm (bỏ dấu, thường)
            string keywordClean = RemoveSign4VietnameseString(keyword);

            // B3: Lọc trong bộ nhớ (In-Memory Filtering)
            var ketqua = allTruyen.Where(s =>
                RemoveSign4VietnameseString(s.TenTruyen).Contains(keywordClean) ||
                RemoveSign4VietnameseString(s.TacGia).Contains(keywordClean)
            ).ToList();

            return View(ketqua);
        }
        private string RemoveSign4VietnameseString(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str.ToLower(); // Chuyển về chữ thường để so sánh
        }

        private readonly string[] VietnameseSigns = new string[]
        {
            "aAeEoOuUiIdDyY","áàạảãâấầậẩẫăắằặẳẵ","ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ","éèẹẻẽêếềệểễ","ÉÈẸẺẼÊẾỀỆỂỄ",
            "óòọỏõôốồộổỗơớờợởỡ","ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ","úùụủũưứừựửữ","ÚÙỤỦŨƯỨỪỰỬỮ","íìịỉĩ","ÍÌỊỈĨ","đ","Đ","ýỳỵỷỹ","ÝỲỴỶỸ"
        };
    }
}