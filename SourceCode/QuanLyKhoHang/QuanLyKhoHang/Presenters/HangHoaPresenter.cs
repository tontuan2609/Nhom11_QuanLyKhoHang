using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Repositories;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang.Presenters
{
    public class HangHoaPresenter
    {
        private readonly IHangHoaView view;
        private readonly HangHoaRepository repository;

        public HangHoaPresenter(IHangHoaView view)
        {
            this.view = view;
            repository = new HangHoaRepository();
            view.ThemClicked += (s, e) => Them();
            view.SuaClicked += (s, e) => Sua();
            view.XoaClicked += (s, e) => Xoa();
            view.LamMoiClicked += (s, e) => view.DatLaiForm();
            view.BoAnhClicked += (s, e) => view.BoAnh();
            view.LocThayDoiClicked += (s, e) => TaiDuLieu();
            try { view.HienThiLoai(repository.LayDanhSachLoai()); }
            catch (Exception ex) { view.HienLoi("Không tải được danh sách loại hàng: " + ex.Message); }
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try { view.HienThiDanhSach(repository.LayDanhSach(view.TuKhoa, view.MaLoaiLoc)); }
            catch (Exception ex) { view.HienLoi("Không tải được dữ liệu: " + ex.Message); }
        }

        private HangHoa LayHangHoa()
        {
            return new HangHoa
            {
                MaHang = view.MaHang.Trim(), TenHang = view.TenHang.Trim(),
                MaLoai = view.MaLoai, DonViTinh = view.DonViTinh.Trim(),
                DonGiaNhap = view.DonGiaNhap, SoLuongTon = view.SoLuongTon,
                MucTonToiThieu = view.MucTonToiThieu, HinhAnh = view.DuongDanAnh
            };
        }

        private bool KiemTra()
        {
            view.XoaLoiNhapLieu();
            bool hopLe = true;
            string ma = view.MaHang.Trim();
            if (ma.Length == 0) { view.BaoLoiMaHang("Vui lòng nhập mã hàng"); hopLe = false; }
            else if (!Regex.IsMatch(ma, "^[A-Za-z0-9_-]+$"))
            { view.BaoLoiMaHang("Mã chỉ gồm chữ không dấu, số, '-' hoặc '_'"); hopLe = false; }
            if (view.TenHang.Trim().Length == 0)
            { view.BaoLoiTenHang("Vui lòng nhập tên hàng"); hopLe = false; }
            if (string.IsNullOrEmpty(view.MaLoai))
            { view.BaoLoiLoai("Vui lòng chọn loại hàng"); hopLe = false; }
            if (view.DonViTinh.Trim().Length == 0)
            { view.BaoLoiDonViTinh("Vui lòng nhập đơn vị tính"); hopLe = false; }
            if (view.DonGiaNhap <= 0)
            { view.BaoLoiDonGiaNhap("Đơn giá nhập phải lớn hơn 0"); hopLe = false; }
            return hopLe;
        }

        private void Them()
        {
            if (view.DangChonHangHoa)
            { view.HienThongBao("Bấm 'Làm mới' trước khi nhập hàng hóa mới."); return; }
            if (!KiemTra()) return;
            HangHoa h = LayHangHoa();
            try
            {
                if (repository.KiemTraMaTonTai(h.MaHang))
                { view.BaoLoiMaHang("Mã hàng đã tồn tại"); return; }
                repository.Them(h);
                TaiDuLieu(); view.ChonDong(h.MaHang);
                view.HienThongBao("Thêm hàng hóa thành công.");
            }
            catch (SqlException ex) { view.HienLoi(UiHelper.MoTaLoiSql(ex)); }
        }

        private void Sua()
        {
            if (!view.DangChonHangHoa)
            { view.HienThongBao("Chọn hàng hóa cần sửa trong danh sách."); return; }
            if (!KiemTra()) return;
            HangHoa h = LayHangHoa();
            try
            {
                if (repository.Sua(h) == 0)
                { view.HienLoi("Không tìm thấy hàng hóa cần sửa."); return; }
                TaiDuLieu(); view.ChonDong(h.MaHang);
                view.HienThongBao("Cập nhật hàng hóa thành công.");
            }
            catch (SqlException ex) { view.HienLoi(UiHelper.MoTaLoiSql(ex)); }
        }

        private void Xoa()
        {
            if (!view.DangChonHangHoa)
            { view.HienThongBao("Chọn hàng hóa cần xóa trong danh sách."); return; }
            string ma = view.MaHang.Trim();
            try
            {
                int soPhieu = repository.DemChiTietPhieu(ma);
                if (soPhieu > 0)
                { view.HienLoi("Không thể xóa: hàng hóa này đã phát sinh " + soPhieu + " dòng nhập/xuất kho."); return; }
                if (!view.XacNhan("Xóa hàng hóa '" + view.TenHang.Trim() + "'?")) return;
                repository.Xoa(ma);
                TaiDuLieu(); view.DatLaiForm();
                view.HienThongBao("Đã xóa hàng hóa.");
            }
            catch (SqlException ex) { view.HienLoi(UiHelper.MoTaLoiSql(ex)); }
        }
    }
}
