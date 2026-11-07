using System;

namespace QuanLyKhoHang.Views
{
    public interface ILoginView
    {
        string TenDangNhap { get; }
        string MatKhau { get; }
        event EventHandler DangNhapClicked;
        void XoaLoiNhapLieu();
        void BaoLoiTenDangNhap(string noiDung);
        void BaoLoiMatKhau(string noiDung);
        void HienLoi(string noiDung);
        void XoaMatKhau();
        void DangNhapThanhCong();
    }
}
