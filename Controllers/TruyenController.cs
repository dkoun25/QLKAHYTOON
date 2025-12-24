using QLKAHYTOON.Models;
using System.Linq;
using System.Web.Mvc;
using System;
using System.Collections.Generic;

namespace QLKAHYTOON.Controllers
{
    public class TruyenController : BaseController
    {
        // Helper method để log
        private void LogDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[TruyenController] {DateTime.Now:HH:mm:ss} - {message}");
        }

        // GET: Truyen/ChiTiet/MT_1
        public ActionResult ChiTiet(string id)
        {
            LogDebug($"ChiTiet called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == id);
            if (truyen == null)
            {
                return HttpNotFound();
            }

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

            var danhSachChuong = db.chuongs
                .Where(c => c.MaTruyen == id)
                .OrderBy(c => c.SoChuong)
                .ToList();
            ViewBag.DanhSachChuong = danhSachChuong;

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

            var soLuotTheoDoi = db.truyenyeuthiches.Count(x => x.MaTruyen == id);
            ViewBag.SoLuotTheoDoi = soLuotTheoDoi;

            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            LogDebug($"User: {(user != null ? user.MaNguoiDung : "null")}, Admin: {(admin != null ? admin.MaAdmin : "null")}, IsAdmin: {isAdmin}");

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
            LogDebug($"DocTruyen called with id: {id}");

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

            var cacChuong = db.chuongs.Where(c => c.MaTruyen == chuong.MaTruyen)
                                      .OrderBy(c => c.SoChuong)
                                      .ToList();

            ViewBag.ListChuong = cacChuong;

            int index = cacChuong.FindIndex(c => c.MaChuong == id);

            if (index > 0) ViewBag.MaChuongTruoc = cacChuong[index - 1].MaChuong.Trim();
            else ViewBag.MaChuongTruoc = null;

            if (index < cacChuong.Count - 1) ViewBag.MaChuongSau = cacChuong[index + 1].MaChuong.Trim();
            else ViewBag.MaChuongSau = null;

            string maNguoiDung = "KHACH";

            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            if (isAdmin && admin != null)
            {
                maNguoiDung = admin.MaAdmin;
                LogDebug($"Admin reading: {maNguoiDung}");
            }
            else if (user != null)
            {
                maNguoiDung = user.MaNguoiDung;
                LogDebug($"User reading: {maNguoiDung}");
            }
            else
            {
                LogDebug("Guest reading");
            }

            try
            {
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
                LogDebug("Reading history saved successfully");
            }
            catch (Exception ex)
            {
                LogDebug($"Error saving reading history: {ex.Message}");
            }

            return View(chuong);
        }

        [HttpPost]
        public JsonResult ThichTruyen(string maTruyen)
        {
            LogDebug($"ThichTruyen called with maTruyen: {maTruyen}");

            // Kiểm tra đăng nhập
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;

            if (user == null && admin == null)
            {
                return Json(new { success = false, msg = "Vui lòng đăng nhập để thích truyện!" });
            }

            // Lấy mã người dùng
            string maNguoiDung = user != null ? user.MaNguoiDung : admin.MaAdmin;

            // Kiểm tra truyện có tồn tại không
            var truyen = db.thongtintruyens.SingleOrDefault(t => t.MaTruyen == maTruyen);
            if (truyen == null)
            {
                return Json(new { success = false, msg = "Truyện không tồn tại!" });
            }

            // Kiểm tra đã thích chưa (dựa vào bảng truyenyeuthich hoặc tạo bảng luotthich riêng)
            // Tạm thời dùng bảng truyenyeuthich để check (nếu follow rồi thì không cho thích thêm)
            var daThich = db.truyenyeuthiches
                .Any(x => x.MaTruyen == maTruyen && x.MaNguoiDung == maNguoiDung);

            if (daThich)
            {
                return Json(new { success = false, msg = "Bạn đã thích truyện này rồi!", alreadyLiked = true });
            }

            // Tăng lượt thích
            if (truyen.LuotThich == null)
            {
                truyen.LuotThich = 0;
            }
            truyen.LuotThich += 1;

            // Thêm vào bảng theo dõi (nếu chưa có)
            var theoDoiMoi = new truyenyeuthich
            {
                MaNguoiDung = maNguoiDung,
                MaTruyen = maTruyen,
                NgayThem = DateTime.Now
            };
            db.truyenyeuthiches.InsertOnSubmit(theoDoiMoi);

            db.SubmitChanges();

            LogDebug($"Liked successfully. Total likes: {truyen.LuotThich}");

            return Json(new
            {
                success = true,
                luotThich = truyen.LuotThich,
                msg = "Đã thích truyện thành công!"
            });
        }

        // THÊM method mới để check đã thích chưa
        [HttpGet]
        public JsonResult CheckDaThich(string maTruyen)
        {
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;

            if (user == null && admin == null)
            {
                return Json(new { daThich = false }, JsonRequestBehavior.AllowGet);
            }

            string maNguoiDung = user != null ? user.MaNguoiDung : admin.MaAdmin;

            var daThich = db.truyenyeuthiches
                .Any(x => x.MaTruyen == maTruyen && x.MaNguoiDung == maNguoiDung);

            return Json(new { daThich = daThich }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult TheoDoiTruyen(string maTruyen)
        {
            LogDebug($"TheoDoiTruyen called with maTruyen: {maTruyen}");

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

            LogDebug($"Follow toggled. Status: {trangThaiTheoDoi}, Total: {soLuotTheoDoi}");

            return Json(new
            {
                success = true,
                daTheoDoi = trangThaiTheoDoi,
                soLuotTheoDoi = soLuotTheoDoi
            });
        }

        [HttpPost]
        public JsonResult RateTruyen(string maTruyen, int rating)
        {
            LogDebug($"RateTruyen called - maTruyen: {maTruyen}, rating: {rating}");

            // Kiểm tra đăng nhập
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;
            bool isAdmin = Session["IsAdmin"] != null && (bool)Session["IsAdmin"];

            LogDebug($"Session check - User: {(user != null)}, Admin: {(admin != null)}, IsAdmin: {isAdmin}");

            if (user == null && admin == null)
            {
                LogDebug("Not logged in - returning error");
                return Json(new { success = false, msg = "Vui lòng đăng nhập để đánh giá!" });
            }

            // Kiểm tra rating hợp lệ (1-5 sao)
            if (rating < 1 || rating > 5)
            {
                LogDebug($"Invalid rating: {rating}");
                return Json(new { success = false, msg = "Đánh giá không hợp lệ!" });
            }

            // Kiểm tra user blocked (nếu không phải admin)
            if (!isAdmin && user != null)
            {
                var userFromDb = db.nguoidungs.SingleOrDefault(u => u.MaNguoiDung == user.MaNguoiDung);
                if (userFromDb == null)
                {
                    LogDebug("User not found in database");
                    Session["User"] = null;
                    return Json(new { success = false, msg = "Tài khoản không tồn tại!", needReload = true });
                }
                if (userFromDb.VaiTro == "Blocked")
                {
                    LogDebug("User is blocked");
                    Session["User"] = userFromDb;
                    return Json(new { success = false, msg = "Tài khoản của bạn đã bị khóa!", isBlocked = true });
                }
                Session["User"] = userFromDb;
            }

            try
            {
                string maNguoiDung = user != null ? user.MaNguoiDung : admin.MaAdmin;
                LogDebug($"Processing rating for user: {maNguoiDung}");

                // Kiểm tra đã đánh giá chưa
                var existingRating = db.danhgias.SingleOrDefault(d =>
                    d.MaTruyen == maTruyen && d.MaNguoiDung == maNguoiDung);

                if (existingRating != null)
                {
                    LogDebug($"Updating existing rating from {existingRating.SoSao} to {rating}");
                    existingRating.SoSao = rating;
                    existingRating.NgayDanhGia = DateTime.Now;
                }
                else
                {
                    LogDebug("Creating new rating");
                    var newRating = new danhgia
                    {
                        MaTruyen = maTruyen,
                        MaNguoiDung = maNguoiDung,
                        SoSao = rating,
                        NgayDanhGia = DateTime.Now
                    };
                    db.danhgias.InsertOnSubmit(newRating);
                }

                db.SubmitChanges();
                LogDebug("Rating saved successfully");

                // Tính điểm trung bình
                var allRatings = db.danhgias.Where(d => d.MaTruyen == maTruyen).ToList();
                double avgRating = allRatings.Any() ? allRatings.Average(d => (double)d.SoSao) : 0;
                int totalRatings = allRatings.Count;

                LogDebug($"Average rating: {avgRating:F2}, Total ratings: {totalRatings}");

                return Json(new
                {
                    success = true,
                    avgRating = avgRating,
                    totalRatings = totalRatings,
                    msg = "Đánh giá thành công!"
                });
            }
            catch (Exception ex)
            {
                LogDebug($"Error in RateTruyen: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetRating(string maTruyen)
        {
            LogDebug($"GetRating called with maTruyen: {maTruyen}");

            try
            {
                var allRatings = db.danhgias.Where(d => d.MaTruyen == maTruyen).ToList();

                double avgRating = 0;
                int totalRatings = allRatings.Count;
                int userRating = 0;

                if (totalRatings > 0)
                {
                    avgRating = allRatings.Average(d => (double)d.SoSao);
                }

                // Lấy đánh giá của user hiện tại
                var user = Session["User"] as nguoidung;
                var admin = Session["User"] as admin;

                if (user != null || admin != null)
                {
                    string maNguoiDung = user != null ? user.MaNguoiDung : admin.MaAdmin;
                    var userRatingObj = db.danhgias.SingleOrDefault(d =>
                        d.MaTruyen == maTruyen && d.MaNguoiDung == maNguoiDung);

                    if (userRatingObj != null)
                    {
                        userRating = userRatingObj.SoSao;
                        LogDebug($"User {maNguoiDung} has rated: {userRating} stars");
                    }
                }

                LogDebug($"GetRating result - Avg: {avgRating:F2}, Total: {totalRatings}, User: {userRating}");

                return Json(new
                {
                    success = true,
                    avgRating = avgRating,
                    totalRatings = totalRatings,
                    userRating = userRating
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogDebug($"Error in GetRating: {ex.Message}");
                return Json(new { success = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}