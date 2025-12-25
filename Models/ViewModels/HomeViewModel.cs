using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QLKAHYTOON.Models;

namespace QLKAHYTOON.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<TruyenCardViewModel> TruyenHotSlider { get; set; }
        public List<TruyenCardViewModel> TruyenDeCu { get; set; }
        public List<TruyenCardViewModel> TruyenMoiCapNhat { get; set; }
        public List<TruyenCardViewModel> TopNgay { get; set; }
        public List<TruyenCardViewModel> TopTuan { get; set; }
        public List<TruyenCardViewModel> TopThang { get; set; }
        public List<theloai> ListTheLoai{ get; set; }
    }
}