using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKAHYTOON.Models;
using System.Configuration;

namespace QLKAHYTOON.Controllers
{
    // Kế thừa từ Controller gốc của MVC
    public class BaseController : Controller
    {
        protected QLKAHYTOONDataContext db;

        public BaseController()
        {
            // Lấy chuỗi kết nối chuẩn từ Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["KAHYToonConnectionString"].ConnectionString;

            db = new QLKAHYTOONDataContext(connectionString);
        }
    }
}