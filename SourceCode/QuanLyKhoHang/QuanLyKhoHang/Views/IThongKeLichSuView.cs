using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using QuanLyKhoHang.Services;

namespace QuanLyKhoHang.Views
{
    /// <summary>
    /// Interface View cho phân hệ Tra cứu lịch sử, Tính tồn kho & Báo cáo biểu đồ
    /// Tuân thủ mô hình MVP (Model - View - Presenter)
    /// </summary>
    public interface IThongKeLichSuView
    {
        // --- 1. Dữ liệu đầu vào từ người dùng (Input) ---
        DateTime TuNgay { get; set; }
        DateTime DenNgay { get; set; }
        string LoaiPhieu { get; }
        string TuKhoaLichSu { get; }

        string MaLoaiHang { get; }
        string TrangThaiTon { get; }
        string TuKhoaTon { get; }

        DateTime BieuDoTuNgay { get; set; }
        DateTime BieuDoDenNgay { get; set; }
        bool LaBieuDoCot { get; }

        // --- 2. Các hàm cập nhật giao diện (Output / Display) ---
        void HienThiDanhMucLoaiHang(DataTable dt);
        void HienThiDanhSachPhieu(DataTable dt);
        void HienThiChiTietPhieu(DataTable dt);
        void HienThiBaoCaoTonKho(DataTable dt);
        void CapNhatBieuDo(List<BieuDoGdiHelper.ItemThongKe> duLieu);

        void CapNhatThongKeLichSu(int soPhieu, decimal tongNhap, decimal tongXuat);
        void CapNhatThongKeTonKho(int soMatHang, decimal tongGiaTriTon, int soCanhBao);

        void HienThongBao(string thongBao);
        void HienThongBaoLoi(string loi);
        IWin32Window GetWindow();
    }
}
