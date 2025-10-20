using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLKAHYTOON.Models.ViewModels
{
    public class TruyenCardViewModel
    {
        public string MaTruyen { get; set; }
        public string TenTruyen { get; set; }
        public string AnhTruyen { get; set; }
        public string TacGia { get; set; }
        public string TenTheLoai { get; set; } // Đây là cột chúng ta cần!
        public string MoTa { get; set; } // Thêm luôn cột MoTa cho slider
    }
}