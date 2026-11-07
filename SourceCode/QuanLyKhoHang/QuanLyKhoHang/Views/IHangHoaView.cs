using System;
using System.Data;

namespace QuanLyKhoHang.Views
{
    public interface IHangHoaView
    {
        string MaHang { get; }
        string TenHang { get; }
        string MaLoai { get; }
        string DonViTinh { get; }
        decimal DonGiaNhap { get; }
        int SoLuongTon { get; }
        int MucTonToiThieu { get; }
        string DuongDanAnh { get; }
        string TuKhoa { get; }
        string MaLoaiLoc { get; }
        bool DangChonHangHoa { get; }
        event EventHandler ThemClicked;
        event EventHandler SuaClicked;
        event EventHandler XoaClicked;
        event EventHandler LamMoiClicked;
        event EventHandler BoAnhClicked;
        event EventHandler LocThayDoiClicked;
        void HienThiLoai(DataTable duLieu);
        void HienThiDanhSach(DataTable duLieu);
        void DatLaiForm();
        void ChonDong(string ma);
        void BoAnh();
        void XoaLoiNhapLieu();
        void BaoLoiMaHang(string s);
        void BaoLoiTenHang(string s);
        void BaoLoiLoai(string s);
        void BaoLoiDonViTinh(string s);
        void BaoLoiDonGiaNhap(string s);
        void HienThongBao(string s);
        void HienLoi(string s);
        bool XacNhan(string s);
    }
}
