using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace QLKAHYTOON.Controllers
{
    namespace QLKAHYTOON.Controllers
    {
        public class TruyenController : Controller
        {
            private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

            // GET: Truyen/ChiTiet/MT_1
            public ActionResult ChiTiet(string id)
            {
                // Lấy thông tin truyện và JOIN với thể loại để lấy tên
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

                ViewBag.DanhSachChuong = db.chuongs.Where(c => c.MaTruyen == id).OrderBy(c => c.SoChuong).ToList();

                // Lấy danh sách bình luận KÈM THEO thông tin người dùng
                var danhSachBinhLuan = from bl in db.binhluans
                                       join nd in db.nguoidungs on bl.MaNguoiDung equals nd.MaNguoiDung
                                       where bl.MaTruyen == id
                                       orderby bl.NgayDang descending
                                       select new
                                       {
                                           bl.NoiDung,
                                           bl.NgayDang,
                                           nd.HoTen, // Lấy họ tên từ bảng nguoidung
                                           nd.Avatar // Lấy luôn avatar
                                       };

                ViewBag.BinhLuan = danhSachBinhLuan.ToList();

                // Truyền cả thông tin truyện và tên thể loại ra View
                ViewBag.TenTheLoai = truyen.TenTheLoai;
                return View(truyen.TruyenInfo);
            }

            // GET: Truyen/DocTruyen/C001
            public ActionResult DocTruyen(string id)
            {
                var chuong = db.chuongs.SingleOrDefault(c => c.MaChuong == id);
                if (chuong == null)
                {
                    return HttpNotFound();
                }
                if (!string.IsNullOrEmpty(chuong.AnhChuong))
                {
                    ViewBag.DanhSachAnh = chuong.AnhChuong.Split(';').ToList();
                }

                // Ghi lại lịch sử đọc nếu người dùng đã đăng nhập
                if (Session["User"] != null)
                {
                    var user = Session["User"] as nguoidung;
                    string maLichSuDoc = "LH" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
                    var lichSu = new lichsudoc
                    {
                        MaLichSuDoc = maLichSuDoc,
                        MaNguoiDung = user.MaNguoiDung,
                        MaTruyen = chuong.MaTruyen,
                        MaChuong = id,
                        ThoiGianDoc = System.DateTime.Now
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
}