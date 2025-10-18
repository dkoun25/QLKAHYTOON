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
                var viewModel = new HomeViewModel();

                // --- Lấy Slider HOT ---
                // (Lưu ý: Bảng của bạn không có MoTa, tôi đang dùng tạm TacGia để thay thế)
                viewModel.TruyenHotSlider = (from t in db.thongtintruyens
                                             join tl in db.theloais on t.MaTheLoai equals tl.MaTheLoai into g
                                             from subTl in g.DefaultIfEmpty() // LEFT JOIN
                                             orderby Guid.NewGuid() // Lấy ngẫu nhiên
                                             select new TruyenCardViewModel
                                             {
                                                 MaTruyen = t.MaTruyen,
                                                 TenTruyen = t.TenTruyen,
                                                 AnhTruyen = t.AnhTruyen,
                                                 TacGia = t.TacGia,
                                                 TenTheLoai = (subTl == null ? "N/A" : subTl.TenTheLoai),
                                                 MoTa = t.TacGia // Tạm dùng TacGia, sau này bạn nên thêm cột MoTa
                                             }).Take(5).ToList();

                // --- Lấy Truyện Đề Cử ---
                viewModel.TruyenDeCu = (from t in db.thongtintruyens
                                        where db.danhgias.Any(d => d.MaTruyen == t.MaTruyen)
                                        orderby db.danhgias.Where(d => d.MaTruyen == t.MaTruyen).Average(d => d.SoSao) descending
                                        join tl in db.theloais on t.MaTheLoai equals tl.MaTheLoai into g
                                        from subTl in g.DefaultIfEmpty()
                                        select new TruyenCardViewModel
                                        {
                                            MaTruyen = t.MaTruyen,
                                            TenTruyen = t.TenTruyen,
                                            AnhTruyen = t.AnhTruyen,
                                            TenTheLoai = (subTl == null ? "N/A" : subTl.TenTheLoai),
                                        }).Take(10).ToList();

                // --- Lấy Truyện Mới Cập Nhật ---
                viewModel.TruyenMoiCapNhat = (from t in db.thongtintruyens
                                              orderby t.NgayDang descending
                                              join tl in db.theloais on t.MaTheLoai equals tl.MaTheLoai into g
                                              from subTl in g.DefaultIfEmpty()
                                              select new TruyenCardViewModel
                                              {
                                                  MaTruyen = t.MaTruyen,
                                                  TenTruyen = t.TenTruyen,
                                                  AnhTruyen = t.AnhTruyen,
                                                  TenTheLoai = (subTl == null ? "N/A" : subTl.TenTheLoai),
                                              }).Take(18).ToList();

                // --- Lấy TOP (Tạm thời, logic sẽ cập nhật sau) ---
                var topTruyenQuery = (from t in db.thongtintruyens
                                      join tl in db.theloais on t.MaTheLoai equals tl.MaTheLoai into g
                                      from subTl in g.DefaultIfEmpty()
                                      select new TruyenCardViewModel
                                      {
                                          MaTruyen = t.MaTruyen,
                                          TenTruyen = t.TenTruyen,
                                          AnhTruyen = t.AnhTruyen,
                                          TenTheLoai = (subTl == null ? "N/A" : subTl.TenTheLoai),
                                      });

                viewModel.TopNgay = topTruyenQuery.OrderByDescending(t => t.TenTruyen).Take(10).ToList();
                viewModel.TopTuan = topTruyenQuery.OrderBy(t => t.TenTruyen).Take(10).ToList();
                viewModel.TopThang = topTruyenQuery.OrderByDescending(t => t.AnhTruyen).Take(10).ToList();

                return View(viewModel);
            }
        }
    }
}