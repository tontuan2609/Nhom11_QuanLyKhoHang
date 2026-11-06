using System;
using System.Collections.Generic;

namespace QuanLyKhoHang.Models
{
    public class PhieuKho
    {
        public string MaPhieu { get; set; }
        public string LoaiPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public string MaNguoiDung { get; set; }
        public string MaNCC { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietPhieu> ChiTiets { get; set; } = new List<ChiTietPhieu>();
    }
}