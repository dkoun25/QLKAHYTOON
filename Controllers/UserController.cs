using QLKAHYTOON.Models;
using QLKAHYTOON.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKAHYTOON.Controllers
{
    // Lớp này dùng để kiểm tra đăng nhập bằng Session
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return httpContext.Session["User"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new { controller = "Account", action = "Login" }));
        }
    }

    [AuthorizeUser] // Sử dụng attribute vừa tạo để kiểm tra đăng nhập
    public class UserController : BaseController
    {
        private nguoidung GetCurrentUser()
        {
            return Session["User"] as nguoidung;
        }

        // GET: User/Profile
        public ActionResult Profile()
        {
            var user = GetCurrentUser();
            // Lấy lại thông tin mới nhất từ DB để đảm bảo dữ liệu luôn đúng
            var userInfo = db.nguoidungs.Single(u => u.MaNguoiDung == user.MaNguoiDung);
            return View(userInfo);
        }

        // GET: User/LichSuDoc
        public ActionResult LichSuDoc()
        {
            var user = GetCurrentUser();
            var lichSu = db.lichsudocs
                           .Where(l => l.MaNguoiDung == user.MaNguoiDung)
                           .OrderByDescending(l => l.ThoiGianDoc) // Sửa thành ThoiGianDoc
                           .ToList();
            return View(lichSu);
        }

        // GET: User/TruyenYeuThich
        public ActionResult TruyenYeuThich()
        {
            var user = GetCurrentUser();
            var yeuThich = db.truyenyeuthiches
                             .Where(yt => yt.MaNguoiDung == user.MaNguoiDung)
                             .OrderByDescending(yt => yt.NgayThem) // Sắp xếp theo ngày thêm
                             .ToList();
            return View(yeuThich);
        }
    }
}