using System;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Repositories;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang.Presenters
{
    public class LoginPresenter
    {
        private readonly ILoginView view;
        private readonly TaiKhoanRepository repository;

        public LoginPresenter(ILoginView view)
        {
            this.view = view;
            repository = new TaiKhoanRepository();
            view.DangNhapClicked += XuLyDangNhap;
        }

        private void XuLyDangNhap(object sender, EventArgs e)
        {
            view.XoaLoiNhapLieu();
            string tenDangNhap = view.TenDangNhap.Trim();
            string matKhau = view.MatKhau;
            bool hopLe = true;

            if (tenDangNhap.Length == 0)
            {
                view.BaoLoiTenDangNhap("Vui lòng nhập tên đăng nhập");
                hopLe = false;
            }
            if (matKhau.Length == 0)
            {
                view.BaoLoiMatKhau("Vui lòng nhập mật khẩu");
                hopLe = false;
            }
            if (!hopLe)
                return;

            try
            {
                ThongTinDangNhap nguoiDung = repository.XacThuc(tenDangNhap, matKhau);
                if (nguoiDung == null)
                {
                    view.HienLoi("Sai tên đăng nhập hoặc mật khẩu, hoặc tài khoản đã bị khóa.");
                    view.XoaMatKhau();
                    return;
                }

                PhienDangNhap.DangNhap(
                    nguoiDung.MaNguoiDung,
                    nguoiDung.TenDangNhap,
                    nguoiDung.HoTen,
                    nguoiDung.VaiTro);
                view.DangNhapThanhCong();
            }
            catch (Exception ex)
            {
                view.HienLoi("Không kết nối được cơ sở dữ liệu. Hãy kiểm tra SQL Server đang chạy và chuỗi kết nối trong App.config.\n\nChi tiết: " + ex.Message);
            }
        }
    }
}
