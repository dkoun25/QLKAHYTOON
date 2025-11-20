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
        public class HomeController : Controller
        {
            private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

            // GET: Home
            public ActionResult Index()
            {
                var listTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
                var viewModel = new HomeViewModel();
                viewModel.ListTheLoai = listTheLoai;
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

                // --- Lấy Truyện Mới Cập Nhật ---
                viewModel.TruyenMoiCapNhat = db.sp_GetTruyenMoiNhat(18).Select(t => new TruyenCardViewModel
                {
                    MaTruyen = t.MaTruyen,
                    TenTruyen = t.TenTruyen,
                    AnhTruyen = t.AnhTruyen,
                    TacGia = t.TacGia,
                    TenTheLoai = t.TenTheLoai,
                    MoTa = t.MoTa
                }).ToList();

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
                                    // Đổi "==" thành ".Contains(id)"
                                    // Thêm kiểm tra t.MaTheLoai != null để tránh lỗi nếu dữ liệu bị rỗng
                                    .Where(t => t.MaTheLoai != null && t.MaTheLoai.Contains(id))
                                    .OrderByDescending(t => t.NgayDang)
                                    .ToList();
                }

                return View(listTruyen);
            }
        }
    }
}