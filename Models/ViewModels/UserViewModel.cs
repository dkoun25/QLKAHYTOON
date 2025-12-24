using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLKAHYTOON.Models.ViewModels
{
    public class TruyenTheoDoiViewModel
    {
        public thongtintruyen Truyen { get; set; }
        public DateTime NgayThem { get; set; }
        public chuong ChapterMoiNhat { get; set; }
        public chuong ChapterDocGanNhat { get; set; }
    }

    public class LichSuDocViewModel
    {
        public thongtintruyen Truyen { get; set; }
        public chuong ChapterDocGanNhat { get; set; }
        public DateTime ThoiGianDoc { get; set; }
    }
}