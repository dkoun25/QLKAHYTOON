using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKAHYTOON.Models;

namespace QLKAHYTOON.Controllers
{
    public class HomeController : Controller
    {
        // Sử dụng DataContext đúng với tên bạn đã tạo
        private QLKAHYTOONDataContext db = new QLKAHYTOONDataContext(System.Configuration.ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString);

        // GET: Home
        public ActionResult Index()
        {
            // Lấy 12 truyện mới nhất dựa vào NgayDang (Giữ nguyên, phần này đã đúng)
            var truyenMoi = db.thongtintruyens.OrderByDescending(t => t.NgayDang).Take(12).ToList();

            // Lấy 12 truyện được đánh giá nhiều nhất (thay cho "xem nhiều")
            var truyenXemNhieu = db.thongtintruyens
                .OrderByDescending(t => db.danhgias.Count(dg => dg.MaTruyen == t.MaTruyen)) // Đếm số đánh giá của truyện này
                .Take(12).ToList();

            // Lấy 6 truyện nổi bật (có điểm đánh giá trung bình cao nhất)
            var truyenNoiBat = db.thongtintruyens
                // Chỉ lấy những truyện có ít nhất một đánh giá
                .Where(t => db.danhgias.Any(d => d.MaTruyen == t.MaTruyen))
                // Sắp xếp giảm dần theo điểm trung bình của các đánh giá (SoSao)
                .OrderByDescending(t => db.danhgias.Where(d => d.MaTruyen == t.MaTruyen).Average(d => d.SoSao))
                .Take(6)
                .ToList();

            // Đưa tất cả vào ViewBag để truyền ra View
            ViewBag.TruyenMoi = truyenMoi;
            ViewBag.TruyenXemNhieu = truyenXemNhieu;
            ViewBag.TruyenNoiBat = truyenNoiBat;

            return View();
        }
    }
}