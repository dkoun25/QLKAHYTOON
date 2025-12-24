using QLKAHYTOON.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    public class BaoCaoController : BaseController
    {
        [HttpPost]
        public JsonResult GuiBaoCao(string maTruyen, string maChuong,string maAdminxuly, string loaiBaoCao, string noiDung)
        {
            // Kiểm tra đăng nhập
            var user = Session["User"] as nguoidung;
            var admin = Session["User"] as admin;

            if (user == null && admin == null)
            {
                return Json(new { success = false, msg = "Vui lòng đăng nhập để gửi báo cáo!" });
            }

            // Kiểm tra nội dung
            if (string.IsNullOrEmpty(noiDung) || string.IsNullOrWhiteSpace(noiDung))
            {
                return Json(new { success = false, msg = "Nội dung báo cáo không được để trống!" });
            }

            try
            {
                string maNguoiDung = user != null ? user.MaNguoiDung : admin.MaAdmin;

                var baoCao = new baocao
                {
                    MaBaoCao = "BC" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    MaNguoiDung = maNguoiDung,
                    MaAdminXuLy = maAdminxuly,
                    MaTruyen = maTruyen,
                    MaChuong = string.IsNullOrEmpty(maChuong) ? null : maChuong,
                    NoiDungBaoCao = noiDung.Trim(),
                    NgayBaoCao = DateTime.Now,
                    TrangThai = "Chưa xử lý"
                };

                db.baocaos.InsertOnSubmit(baoCao);
                db.SubmitChanges();

                return Json(new { success = true, msg = "Gửi báo cáo thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, msg = "Có lỗi xảy ra: " + ex.Message });
            }
        }
    }
}