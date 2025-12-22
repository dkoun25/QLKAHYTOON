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

            // 1. Lấy thông tin truyện
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == id);
            if (truyen == null)
            {
                return HttpNotFound();
            }

            // 2. Danh sách thể loại (nhiều thể loại)
            var danhSachTheLoai = new List<theloai>();
            if (!string.IsNullOrEmpty(truyen.MaTheLoai))
            {
                var maTheLoaiArray = truyen.MaTheLoai
                    .Split(',')
                    .Select(m => m.Trim())
                    .ToList();

                danhSachTheLoai = db.theloais
                    .Where(tl => maTheLoaiArray.Contains(tl.MaTheLoai))
                    .ToList();
            }
            ViewBag.DanhSachTheLoai = danhSachTheLoai;

            // 3. Danh sách chương
            var danhSachChuong = db.chuongs
                .Where(c => c.MaTruyen == id)
                .OrderBy(c => c.SoChuong)
                .ToList();
            ViewBag.DanhSachChuong = danhSachChuong;

            // 4. Bình luận + người dùng
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

            // 5. Đếm lượt theo dõi
            var soLuotTheoDoi = db.truyenyeuthiches.Count(x => x.MaTruyen == id);
            ViewBag.SoLuotTheoDoi = soLuotTheoDoi;

            // 6. Kiểm tra user đã theo dõi chưa
            var user = Session["User"] as nguoidung;
            if (user != null)
            {
                var daTheoDoi = db.truyenyeuthiches
                    .Any(x => x.MaTruyen == id && x.MaNguoiDung == user.MaNguoiDung);
                ViewBag.DaTheoDoi = daTheoDoi;
            }
            else
            {
                ViewBag.DaTheoDoi = false;
            }

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

        
        [HttpPost]
        public JsonResult ThichTruyen(string maTruyen)
        {
            if (Session["User"] == null)
            {
                return Json(new { success = false, msg = "Vui lòng đăng nhập!" });
            }

            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null)
            {
                return Json(new { success = false, msg = "Truyện không tồn tại!" });
            }

            // Nếu null thì set = 0 trước
            if (truyen.LuotThich == null)
            {
                truyen.LuotThich = 0;
            }

            truyen.LuotThich += 1;
            db.SubmitChanges();

            return Json(new
            {
                success = true,
                luotThich = truyen.LuotThich
            });
        }
        [HttpPost]
        public JsonResult TheoDoiTruyen(string maTruyen)
        {
            var user = Session["User"] as nguoidung;
            if (user == null)
            {
                return Json(new { success = false, msg = "Vui lòng đăng nhập!" });
            }
            var daTheoDoi = db.truyenyeuthiches
                .SingleOrDefault(x => x.MaTruyen == maTruyen && x.MaNguoiDung == user.MaNguoiDung);
            bool trangThaiTheoDoi = false;
            if (daTheoDoi == null)
            {
                var theoDoiMoi = new truyenyeuthich
                {
                    MaNguoiDung = user.MaNguoiDung,
                    MaTruyen = maTruyen,
                    NgayThem = DateTime.Now
                };

                db.truyenyeuthiches.InsertOnSubmit(theoDoiMoi);
                trangThaiTheoDoi = true;
            }
            else
            {
                db.truyenyeuthiches.DeleteOnSubmit(daTheoDoi);
                trangThaiTheoDoi = false;
            }
            db.SubmitChanges();
            var soLuotTheoDoi = db.truyenyeuthiches.Count(x => x.MaTruyen == maTruyen);
            return Json(new
            {
                success = true,
                daTheoDoi = trangThaiTheoDoi,
                soLuotTheoDoi = soLuotTheoDoi
            });
        }



    }
}