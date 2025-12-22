using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
                var allTruyenMoiCapNhat = db.sp_GetTruyenMoiNhat(1000).ToList();

                // Tính tổng số trang
                int totalTruyen = allTruyenMoiCapNhat.Count();
                int totalPages = (int)Math.Ceiling((double)totalTruyen / pageSize);

                // Lấy truyện theo trang hiện tại
                var pagedTruyen = allTruyenMoiCapNhat
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Chuyển đổi sang ViewModel và load chapters
                viewModel.TruyenMoiCapNhat = new List<TruyenCardViewModel>();

                foreach (var t in pagedTruyen)
                {
                    var truyenVM = new TruyenCardViewModel
                    {
                        MaTruyen = t.MaTruyen,
                        TenTruyen = t.TenTruyen,
                        AnhTruyen = t.AnhTruyen,
                        TacGia = t.TacGia,
                        TenTheLoai = t.TenTheLoai,
                        MoTa = t.MoTa,
                        MaTheLoai = t.MaTheLoai
                    };

                    // Lấy 3 chapter mới nhất của truyện này
                    var top3Chapters = db.chuongs
                        .Where(c => c.MaTruyen == t.MaTruyen)
                        .OrderByDescending(c => c.SoChuong)
                        .Take(3)
                        .Select(c => new ChapterViewModel
                        {
                            MaChuong = c.MaChuong,
                            TenChuong = c.TenChuong,
                            SoChuong = c.SoChuong
                        })
                        .ToList();

                    truyenVM.Top3Chapters = top3Chapters;
                    viewModel.TruyenMoiCapNhat.Add(truyenVM);
                }

                // Truyền thông tin phân trang qua ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;

                // --- Lấy TOP với Chapter mới nhất ---
                var topNgayData = db.sp_GetTopTruyenByView("Ngay").ToList();
                viewModel.TopNgay = topNgayData.Select(sp_result => {
                    var latestChapter = db.chuongs
                        .Where(c => c.MaTruyen == sp_result.MaTruyen)
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault();

                    return new TruyenCardViewModel
                    {
                        MaTruyen = sp_result.MaTruyen,
                        TenTruyen = sp_result.TenTruyen,
                        AnhTruyen = sp_result.AnhTruyen,
                        TenTheLoai = sp_result.TenTheLoai,
                        SoChuongMoiNhat = latestChapter?.SoChuong,
                        MaChuongMoiNhat = latestChapter?.MaChuong
                    };
                }).ToList();

                var topTuanData = db.sp_GetTopTruyenByView("Tuan").ToList();
                viewModel.TopTuan = topTuanData.Select(sp_result => {
                    var latestChapter = db.chuongs
                        .Where(c => c.MaTruyen == sp_result.MaTruyen)
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault();

                    return new TruyenCardViewModel
                    {
                        MaTruyen = sp_result.MaTruyen,
                        TenTruyen = sp_result.TenTruyen,
                        AnhTruyen = sp_result.AnhTruyen,
                        TenTheLoai = sp_result.TenTheLoai,
                        SoChuongMoiNhat = latestChapter?.SoChuong,
                        MaChuongMoiNhat = latestChapter?.MaChuong
                    };
                }).ToList();

                var topThangData = db.sp_GetTopTruyenByView("Thang").ToList();
                viewModel.TopThang = topThangData.Select(sp_result => {
                    var latestChapter = db.chuongs
                        .Where(c => c.MaTruyen == sp_result.MaTruyen)
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault();

                    return new TruyenCardViewModel
                    {
                        MaTruyen = sp_result.MaTruyen,
                        TenTruyen = sp_result.TenTruyen,
                        AnhTruyen = sp_result.AnhTruyen,
                        TenTheLoai = sp_result.TenTheLoai,
                        SoChuongMoiNhat = latestChapter?.SoChuong,
                        MaChuongMoiNhat = latestChapter?.MaChuong
                    };
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
            public ActionResult TimKiem(string keyword)
            {
                // Lấy tất cả thể loại để hiển thị trong view
                var allTheLoai = db.theloais.OrderBy(tl => tl.TenTheLoai).ToList();
                ViewBag.AllTheLoai = allTheLoai;
                ViewBag.TuKhoa = keyword ?? "";

                List<thongtintruyen> ketQua = new List<thongtintruyen>();

                if (!string.IsNullOrEmpty(keyword))
                {
                    // Bỏ dấu từ khóa tìm kiếm
                    var keywordKhongDau = RemoveDiacritics(keyword);

                    // Lấy tất cả truyện và filter trong memory
                    var allTruyen = db.thongtintruyens.ToList();

                    // Tìm kiếm không dấu
                    ketQua = allTruyen
                        .Where(t => RemoveDiacritics(t.TenTruyen).Contains(keywordKhongDau))
                        .OrderByDescending(t => t.NgayDang)
                        .ToList();
                }

                return View(ketQua);
            }

            // Thay thế action GetSearchSuggestions trong HomeController.cs
            private string RemoveDiacritics(string text)
            {
                if (string.IsNullOrEmpty(text))
                    return text;

                // Normalize về dạng FormD (tách ký tự và dấu)
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    // Bỏ qua các dấu (NonSpacingMark)
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString()
                                    .Normalize(NormalizationForm.FormC)
                                    .ToLower(); // Chuyển về chữ thường để so sánh
            }
            [HttpGet]
            public ActionResult GetSearchSuggestions(string keyword)
            {
                // 1. Kiểm tra từ khóa
                if (string.IsNullOrEmpty(keyword))
                {
                    return Json(new { data = new object[] { } }, JsonRequestBehavior.AllowGet);
                }

                // 2. Bỏ dấu từ khóa tìm kiếm
                var keywordKhongDau = RemoveDiacritics(keyword);

                // 3. Lấy tất cả truyện và filter trong memory (để có thể bỏ dấu)
                var allTruyen = db.thongtintruyens.ToList();

                // 4. Tìm kiếm không dấu
                var listTruyen = allTruyen
                    .Where(t => RemoveDiacritics(t.TenTruyen).Contains(keywordKhongDau))
                    .OrderByDescending(t => t.NgayDang)
                    .Take(5)
                    .ToList();

                // 5. Chọn lọc dữ liệu để trả về
                var ketQua = listTruyen.Select(t => {
                    var latestChapter = t.chuongs
                        .OrderByDescending(c => c.SoChuong)
                        .FirstOrDefault();

                    string chapterText = "Đang cập nhật";
                    if (latestChapter != null)
                    {
                        // CHỈ HIỂN THỊ "Chương X" - KHÔNG CÓ TÊN CHƯƠNG
                        chapterText = "Chương " + latestChapter.SoChuong;
                    }

                    return new
                    {
                        t.MaTruyen,
                        t.TenTruyen,
                        t.AnhTruyen,
                        ChapterMoi = chapterText
                    };
                });

                // 6. Trả về JSON
                return Json(new { data = ketQua }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}