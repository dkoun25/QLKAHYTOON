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
        public class HomeController : BaseController
        {
            // GET: Home
            public ActionResult Index(int? page)
            {
                // Cấu hình phân trang
                int pageSize = 12; // Hiển thị 12 truyện mỗi trang
                int pageNumber = (page ?? 1); // Nếu page là null thì mặc định là trang 1

                var listTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
                var viewModel = new HomeViewModel();
                viewModel.ListTheLoai = listTheLoai;
                ViewBag.AllTheLoai = listTheLoai;

                viewModel.TruyenHotSlider = db.sp_GetTruyenHotSlider().Select(t => new TruyenCardViewModel
                {
                    MaTruyen = t.MaTruyen,
                    TenTruyen = t.TenTruyen,
                    AnhTruyen = t.AnhTruyen,
                    TacGia = t.TacGia,
                    MaTheLoai = t.MaTheLoai,
                    TenTheLoai = t.TenTheLoai,
                    MoTa = t.MoTa
                }).ToList();

                // --- Lấy Truyện Đề Cử ---
                viewModel.TruyenDeCu = db.sp_GetTruyenDeCu().Select(t => new TruyenCardViewModel
                {
                    MaTruyen = t.MaTruyen,
                    TenTruyen = t.TenTruyen,
                    AnhTruyen = t.AnhTruyen,
                    TacGia = t.TacGia,
                    TenTheLoai = t.TenTheLoai,
                    MoTa = t.MoTa
                }).ToList();

                // --- Lấy Truyện Mới Cập Nhật CÓ PHÂN TRANG ---
                var allTruyenMoiCapNhat = db.sp_GetTruyenMoiNhat(1000).ToList(); // Lấy nhiều để phân trang

                // Tính tổng số trang
                int totalTruyen = allTruyenMoiCapNhat.Count();
                int totalPages = (int)Math.Ceiling((double)totalTruyen / pageSize);

                // Lấy truyện theo trang hiện tại
                viewModel.TruyenMoiCapNhat = allTruyenMoiCapNhat
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TruyenCardViewModel
                    {
                        MaTruyen = t.MaTruyen,
                        TenTruyen = t.TenTruyen,
                        AnhTruyen = t.AnhTruyen,
                        TacGia = t.TacGia,
                        TenTheLoai = t.TenTheLoai,
                        MoTa = t.MoTa,
                        MaTheLoai = t.MaTheLoai                      
                    }).ToList();

                // Truyền thông tin phân trang qua ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                // --- Lấy TOP (Tạm thời) ---
                viewModel.TopNgay = db.sp_GetTopTruyenByView("Ngay").Select(sp_result => new TruyenCardViewModel
                {
                    MaTruyen = sp_result.MaTruyen,
                    TenTruyen = sp_result.TenTruyen,
                    AnhTruyen = sp_result.AnhTruyen,
                    TenTheLoai = sp_result.TenTheLoai,
                }).ToList();

                viewModel.TopTuan = db.sp_GetTopTruyenByView("Tuan").Select(sp_result => new TruyenCardViewModel
                {
                    MaTruyen = sp_result.MaTruyen,
                    TenTruyen = sp_result.TenTruyen,
                    AnhTruyen = sp_result.AnhTruyen,
                    TenTheLoai = sp_result.TenTheLoai,
                }).ToList();

                viewModel.TopThang = db.sp_GetTopTruyenByView("Thang").Select(sp_result => new TruyenCardViewModel
                {
                    MaTruyen = sp_result.MaTruyen,
                    TenTruyen = sp_result.TenTruyen,
                    AnhTruyen = sp_result.AnhTruyen,
                    TenTheLoai = sp_result.TenTheLoai,
                }).ToList();

                return View(viewModel);
            }

            public ActionResult TheLoai(string id)
            {
                // 1. Lấy danh sách TẤT CẢ thể loại để hiển thị lên thanh menu ngang
                var allTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
                ViewBag.AllTheLoai = allTheLoai;

                List<thongtintruyen> listTruyen;

                if (string.IsNullOrEmpty(id))
                {
                    ViewBag.TenTheLoai = "Tất Cả Truyện";
                    ViewBag.ActiveId = "";

                    listTruyen = db.thongtintruyens.OrderByDescending(t => t.NgayDang).ToList();
                }
                else
                {
                    var theLoai = db.theloais.SingleOrDefault(tl => tl.MaTheLoai == id);
                    if (theLoai == null) return HttpNotFound();

                    ViewBag.TenTheLoai = theLoai.TenTheLoai;
                    ViewBag.ActiveId = id;

                    listTruyen = db.thongtintruyens
                                    .Where(t => t.MaTheLoai != null && t.MaTheLoai.Contains(id))
                                    .OrderByDescending(t => t.NgayDang)
                                    .ToList();
                }

                return View(listTruyen);
            }
            [HttpGet]
            public ActionResult GetSearchSuggestions(string keyword) // <--- Đổi thành ActionResult
            {
                // 1. Kiểm tra từ khóa
                if (string.IsNullOrEmpty(keyword))
                {
                    return Json(new { data = new object[] { } }, JsonRequestBehavior.AllowGet);
                }

                // 2. Truy vấn Database
                // Lưu ý: db.Truyens có thể là db.Truyen (tùy cách bạn đặt tên trong Model)
                var query = db.thongtintruyens.Where(t => t.TenTruyen.Contains(keyword));

                // Sắp xếp và lấy 5 tin
                var listTruyen = query.OrderByDescending(t => t.NgayDang).Take(5).ToList();

                // 3. Chọn lọc dữ liệu để trả về (Chỉ lấy cái cần thiết)
                var ketQua = listTruyen.Select(t => new {
                    t.MaTruyen,      // Hoặc t.Id
                    t.TenTruyen,
                    t.AnhTruyen,        // Link ảnh
                                     // Lấy tên chapter mới nhất (xử lý lỗi nếu chưa có chapter nào)
                    ChapterMoi = t.chuongs.OrderByDescending(c => c.SoChuong).FirstOrDefault() != null
                                 ? t.chuongs.OrderByDescending(c => c.SoChuong).FirstOrDefault().TenChuong
                                 : "Mới"
                });
                // 4. Trả về JSON (QUAN TRỌNG: Phải có AllowGet)
                return Json(new { data = ketQua }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}