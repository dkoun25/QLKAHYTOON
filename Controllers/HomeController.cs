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
                // Lấy 12 truyện mới nhất dựa vào NgayDang
                var truyenMoi = db.thongtintruyens.OrderByDescending(t => t.NgayDang).Take(12).ToList();

                // Lấy 12 truyện được đánh giá nhiều nhất
                var truyenXemNhieu = db.thongtintruyens
                    .OrderByDescending(t => db.danhgias.Count(dg => dg.MaTruyen == t.MaTruyen))
                    .Take(12).ToList();

                // Lấy 6 truyện nổi bật (có điểm đánh giá trung bình cao nhất)
                var truyenNoiBat = db.thongtintruyens
                    .Where(t => db.danhgias.Any(d => d.MaTruyen == t.MaTruyen))
                    .OrderByDescending(t => db.danhgias.Where(d => d.MaTruyen == t.MaTruyen).Average(d => d.SoSao))
                    .Take(6)
                    .ToList();

                ViewBag.TruyenMoi = truyenMoi;
                ViewBag.TruyenXemNhieu = truyenXemNhieu;
                ViewBag.TruyenNoiBat = truyenNoiBat;

                return View();
            }
        }
    }
}