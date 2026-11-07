
using System;
using System.Data;

namespace QuanLyKhoHang.Views
{
    public interface ILoaiHangView
    {
        string MaLoai { get; set; }
        string TenLoai { get; set; }
        string MoTa { get; set; }
        string TuKhoa { get; }

        bool DangChonLoaiHang { get; }

        event EventHandler ThemClicked;
        event EventHandler SuaClicked;
        event EventHandler XoaClicked;
        event EventHandler LamMoiClicked;
        event EventHandler TimKiemChanged;

        void HienThiDanhSach(DataTable duLieu);
        void ChonDong(string ma);
        void DatLaiForm();

        void HienThongBao(string noiDung);
        void HienLoi(string noiDung);
        bool XacNhan(string noiDung);

        void BaoLoiMaLoai(string noiDung);
        void BaoLoiTenLoai(string noiDung);
        void XoaLoiNhapLieu();
    }
}
