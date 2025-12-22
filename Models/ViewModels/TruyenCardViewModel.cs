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
        public string MaTheLoai { get; set; }
        public string TenTheLoai { get; set; }
        public string MoTa { get; set; }
        public int? SoChuongMoiNhat { get; set; }
        public string MaChuongMoiNhat { get; set; }

        // Danh sách 3 chapter mới nhất
        public List<ChapterViewModel> Top3Chapters { get; set; }
    }

    public class ChapterViewModel
    {
        public string MaChuong { get; set; }
        public string TenChuong { get; set; }
        public int? SoChuong { get; set; }
    }
}