
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using QuanLyKhoHang.Views;
using QuanLyKhoHang.Repositories;

namespace QuanLyKhoHang.Presenters
{
    public class LoaiHangPresenter
    {
        private readonly ILoaiHangView view;
        private readonly LoaiHangRepository repository;

        public LoaiHangPresenter(ILoaiHangView view)
        {
            this.view = view;
            repository = new LoaiHangRepository();

            view.ThemClicked += Them;
            view.SuaClicked += Sua;
            view.XoaClicked += Xoa;
            view.LamMoiClicked += LamMoi;
            view.TimKiemChanged += TimKiem;

            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            try
            {
                view.HienThiDanhSach(
                    repository.LayDanhSach(view.TuKhoa));
            }
            catch (Exception ex)
            {
                view.HienLoi("Không tải được dữ liệu: " + ex.Message);
            }
        }

        private bool KiemTra()
        {
            view.XoaLoiNhapLieu();
            bool hopLe = true;

            string ma = view.MaLoai.Trim();

            if (ma.Length == 0)
            {
                view.BaoLoiMaLoai("Vui lòng nhập mã loại");
                hopLe = false;
            }
            else if (!Regex.IsMatch(ma, "^[A-Za-z0-9_-]+$"))
            {
                view.BaoLoiMaLoai(
                    "Mã chỉ gồm chữ không dấu, số, '-' hoặc '_'");
                hopLe = false;
            }

            if (view.TenLoai.Trim().Length == 0)
            {
                view.BaoLoiTenLoai("Vui lòng nhập tên loại");
                hopLe = false;
            }

            return hopLe;
        }

        private void Them(object sender, EventArgs e)
        {
            if (view.DangChonLoaiHang)
            {
                view.HienThongBao(
                    "Bấm 'Làm mới' trước khi nhập loại hàng mới.");
                return;
            }

            if (!KiemTra())
                return;

            string ma = view.MaLoai.Trim();

            try
            {
                if (repository.KiemTraMaTonTai(ma))
                {
                    view.BaoLoiMaLoai("Mã loại đã tồn tại");
                    return;
                }

                repository.Them(
                    ma, view.TenLoai.Trim(), view.MoTa.Trim());

                TaiDuLieu();
                view.ChonDong(ma);
                view.HienThongBao("Thêm loại hàng thành công.");
            }
            catch (SqlException ex)
            {
                view.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void Sua(object sender, EventArgs e)
        {
            if (!view.DangChonLoaiHang)
            {
                view.HienThongBao(
                    "Chọn loại hàng cần sửa trong danh sách.");
                return;
            }

            if (!KiemTra())
                return;

            string ma = view.MaLoai.Trim();

            try
            {
                int soDong = repository.Sua(
                    ma, view.TenLoai.Trim(), view.MoTa.Trim());

                if (soDong == 0)
                {
                    view.HienLoi("Không tìm thấy loại hàng cần sửa.");
                    return;
                }

                TaiDuLieu();
                view.ChonDong(ma);
                view.HienThongBao(
                    "Cập nhật loại hàng thành công.");
            }
            catch (SqlException ex)
            {
                view.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void Xoa(object sender, EventArgs e)
        {
            if (!view.DangChonLoaiHang)
            {
                view.HienThongBao(
                    "Chọn loại hàng cần xóa trong danh sách.");
                return;
            }

            string ma = view.MaLoai.Trim();

            try
            {
                int soHang = repository.DemHangHoa(ma);

                if (soHang > 0)
                {
                    view.HienLoi(
                        "Không thể xóa: loại hàng này đang có " +
                        soHang + " hàng hóa.");
                    return;
                }

                if (!view.XacNhan(
                    "Xóa loại hàng '" + view.TenLoai.Trim() + "'?"))
                    return;

                repository.Xoa(ma);
                TaiDuLieu();
                view.DatLaiForm();
                view.HienThongBao("Đã xóa loại hàng.");
            }
            catch (SqlException ex)
            {
                view.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void LamMoi(object sender, EventArgs e)
        {
            view.DatLaiForm();
        }

        private void TimKiem(object sender, EventArgs e)
        {
            TaiDuLieu();
        }
    }
}