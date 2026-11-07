using System;

namespace QuanLyKhoHang.Models
{
    /// <summary>
    /// Model dữ liệu thống kê tồn kho của mặt hàng
    /// </summary>
    public class TonKhoItemModel
    {
        public string MaHang { get; set; }
        public string TenHang { get; set; }
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string DonViTinh { get; set; }
        public decimal DonGiaNhap { get; set; }
        public int TongNhap { get; set; }
        public int TongXuat { get; set; }
        public int SoLuongTon { get; set; }
        public int MucTonToiThieu { get; set; }
        public decimal GiaTriTon { get; set; }
        public string TrangThai { get; set; }
    }

    /// <summary>
    /// Model dữ liệu tóm tắt phiếu kho phục vụ tra cứu lịch sử
    /// </summary>
    public class LichSuPhieuItemModel
    {
        public string MaPhieu { get; set; }
        public string LoaiPhieu { get; set; }
        public DateTime NgayLap { get; set; }
        public string NguoiLap { get; set; }
        public string DoiTac { get; set; }
        public int SoMatHang { get; set; }
        public int TongSoLuong { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
    }
}
