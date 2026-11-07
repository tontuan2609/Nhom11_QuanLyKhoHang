using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using QuanLyKhoHang.Services;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang.Presenters
{
    /// <summary>
    /// Presenter cho phân hệ Thống kê, Tra cứu lịch sử & Biểu đồ
    /// Tuân thủ chuẩn MVP (Model - View - Presenter):
    /// Điều phối luồng xử lý, truy xuất dữ liệu/Stored Procedure và chỉ thị View hiển thị kết quả.
    /// </summary>
    public class ThongKeLichSuPresenter
    {
        private readonly IThongKeLichSuView _view;

        public ThongKeLichSuPresenter(IThongKeLichSuView view)
        {
            if (view == null) throw new ArgumentNullException("view");
            _view = view;
        }

        public void NapDanhMucLoaiHang()
        {
            try
            {
                DataTable dt = Database.GetData("SELECT MaLoai, TenLoai FROM LoaiHang ORDER BY TenLoai");
                DataRow rAll = dt.NewRow();
                rAll["MaLoai"] = "";
                rAll["TenLoai"] = "-- Tất cả loại hàng --";
                dt.Rows.InsertAt(rAll, 0);

                _view.HienThiDanhMucLoaiHang(dt);
            }
            catch (Exception ex)
            {
                _view.HienThongBaoLoi("Lỗi nạp danh mục loại hàng: " + ex.Message);
            }
        }

        public void TaiLichSuNhapXuat()
        {
            try
            {
                DateTime tuNgay = _view.TuNgay.Date;
                DateTime denNgayChuan = _view.DenNgay.Date.AddDays(1);
                string loaiPhieu = _view.LoaiPhieu;
                string tuKhoa = _view.TuKhoaLichSu.Trim();

                DataTable dt;
                try
                {
                    dt = Database.GetDataProc("sp_TraCuuLichSuNhapXuat",
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgay", denNgayChuan),
                        Database.Param("@LoaiPhieu", loaiPhieu),
                        Database.Param("@TuKhoa", tuKhoa));
                }
                catch
                {
                    string sqlFallback = @"
                        SELECT 
                            p.MaPhieu,
                            p.LoaiPhieu,
                            p.NgayLap,
                            nd.HoTen AS NguoiLap,
                            ISNULL(ncc.TenNCC, N'(Khách lẻ / Đơn vị ngoài)') AS DoiTac,
                            COUNT(ct.MaHang) AS SoMatHang,
                            ISNULL(SUM(ct.SoLuong), 0) AS TongSoLuong,
                            p.TongTien,
                            p.GhiChu
                        FROM PhieuKho p
                        INNER JOIN NguoiDung nd ON p.MaNguoiDung = nd.MaNguoiDung
                        LEFT JOIN NhaCungCap ncc ON p.MaNCC = ncc.MaNCC
                        LEFT JOIN ChiTietPhieu ct ON p.MaPhieu = ct.MaPhieu
                        WHERE p.NgayLap >= @TuNgay AND p.NgayLap < @DenNgayChuan
                          AND (@LoaiPhieu = N'Tất cả' OR p.LoaiPhieu = @LoaiPhieu)
                          AND (
                              @TuKhoa = N'' 
                              OR p.MaPhieu LIKE '%' + @TuKhoa + '%'
                              OR nd.HoTen LIKE N'%' + @TuKhoa + '%'
                              OR ISNULL(ncc.TenNCC, '') LIKE N'%' + @TuKhoa + '%'
                              OR ISNULL(p.GhiChu, '') LIKE N'%' + @TuKhoa + '%'
                          )
                        GROUP BY p.MaPhieu, p.LoaiPhieu, p.NgayLap, nd.HoTen, ncc.TenNCC, p.TongTien, p.GhiChu
                        ORDER BY p.NgayLap DESC";

                    dt = Database.GetData(sqlFallback,
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgayChuan", denNgayChuan),
                        Database.Param("@LoaiPhieu", loaiPhieu),
                        Database.Param("@TuKhoa", tuKhoa));
                }

                _view.HienThiDanhSachPhieu(dt);

                decimal tongNhap = 0;
                decimal tongXuat = 0;
                foreach (DataRow r in dt.Rows)
                {
                    string lp = r["LoaiPhieu"].ToString();
                    decimal tien = Convert.ToDecimal(r["TongTien"]);
                    if (lp == "Nhập") tongNhap += tien;
                    else if (lp == "Xuất") tongXuat += tien;
                }

                _view.CapNhatThongKeLichSu(dt.Rows.Count, tongNhap, tongXuat);
            }
            catch (Exception ex)
            {
                _view.HienThongBaoLoi("Lỗi khi tải lịch sử: " + ex.Message);
            }
        }

        public void TaiChiTietPhieu(string maPhieu)
        {
            if (string.IsNullOrEmpty(maPhieu))
            {
                _view.HienThiChiTietPhieu(null);
                return;
            }

            try
            {
                DataTable dt;
                try
                {
                    dt = Database.GetDataProc("sp_LayChiTietPhieuDeIn", Database.Param("@MaPhieu", maPhieu));
                }
                catch
                {
                    string sql = @"
                        SELECT 
                            ct.MaHang,
                            h.TenHang,
                            l.TenLoai,
                            h.DonViTinh,
                            ct.SoLuong,
                            ct.DonGia,
                            ct.ThanhTien
                        FROM ChiTietPhieu ct
                        INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                        INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
                        WHERE ct.MaPhieu = @MaPhieu
                        ORDER BY ct.MaHang ASC";

                    dt = Database.GetData(sql, Database.Param("@MaPhieu", maPhieu));
                }

                _view.HienThiChiTietPhieu(dt);
            }
            catch (Exception ex)
            {
                _view.HienThongBaoLoi("Lỗi khi tải chi tiết phiếu: " + ex.Message);
            }
        }

        public void TaiBaoCaoTonKho()
        {
            try
            {
                string maLoai = _view.MaLoaiHang;
                string trangThai = _view.TrangThaiTon;
                string tuKhoa = _view.TuKhoaTon.Trim();

                DataTable dt;
                try
                {
                    dt = Database.GetDataProc("sp_TinhTonKhoChiTiet",
                        Database.Param("@MaLoai", maLoai),
                        Database.Param("@TuKhoa", tuKhoa),
                        Database.Param("@TrangThaiTon", trangThai));
                }
                catch
                {
                    string sqlFallback = @"
                        WITH BangThongKe AS (
                            SELECT 
                                h.MaHang,
                                h.TenHang,
                                h.MaLoai,
                                l.TenLoai,
                                h.DonViTinh,
                                h.DonGiaNhap,
                                h.SoLuongTon,
                                h.MucTonToiThieu,
                                CAST(h.SoLuongTon * h.DonGiaNhap AS DECIMAL(18,2)) AS GiaTriTon,
                                ISNULL((
                                    SELECT SUM(ct.SoLuong)
                                    FROM ChiTietPhieu ct
                                    INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                                    WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Nhập'
                                ), 0) AS TongNhap,
                                ISNULL((
                                    SELECT SUM(ct.SoLuong)
                                    FROM ChiTietPhieu ct
                                    INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                                    WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Xuất'
                                ), 0) AS TongXuat,
                                CASE 
                                    WHEN h.SoLuongTon = 0 THEN N'Hết hàng'
                                    WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
                                    ELSE N'An toàn'
                                END AS TrangThai
                            FROM HangHoa h
                            INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
                            WHERE (@MaLoai = '' OR h.MaLoai = @MaLoai)
                              AND (@TuKhoa = N'' OR h.MaHang LIKE '%' + @TuKhoa + '%' OR h.TenHang LIKE N'%' + @TuKhoa + '%')
                        )
                        SELECT * FROM BangThongKe
                        WHERE (@TrangThaiTon = N'Tất cả' OR TrangThai = @TrangThaiTon)
                        ORDER BY 
                            CASE TrangThai WHEN N'Hết hàng' THEN 1 WHEN N'Cảnh báo tồn thấp' THEN 2 ELSE 3 END,
                            MaHang ASC";

                    dt = Database.GetData(sqlFallback,
                        Database.Param("@MaLoai", maLoai),
                        Database.Param("@TuKhoa", tuKhoa),
                        Database.Param("@TrangThaiTon", trangThai));
                }

                _view.HienThiBaoCaoTonKho(dt);

                decimal tongGiaTri = 0;
                int soCanhBao = 0;
                foreach (DataRow r in dt.Rows)
                {
                    tongGiaTri += Convert.ToDecimal(r["GiaTriTon"]);
                    string tt = r["TrangThai"].ToString();
                    if (tt == "Cảnh báo tồn thấp" || tt == "Hết hàng") soCanhBao++;
                }

                _view.CapNhatThongKeTonKho(dt.Rows.Count, tongGiaTri, soCanhBao);
            }
            catch (Exception ex)
            {
                _view.HienThongBaoLoi("Lỗi tính tồn kho qua Stored Procedure: " + ex.Message);
            }
        }

        public void TaiDuLieuBieuDo()
        {
            try
            {
                DateTime tuNgay = _view.BieuDoTuNgay.Date;
                DateTime denNgayChuan = _view.BieuDoDenNgay.Date.AddDays(1);

                DataTable dt;
                try
                {
                    dt = Database.GetDataProc("sp_ThongKeNhapXuatTon",
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgay", denNgayChuan));
                }
                catch
                {
                    string sqlFallback = @"
                        SELECT 
                            h.MaHang,
                            h.TenHang,
                            ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Nhập' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongNhap,
                            ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Xuất' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongXuat,
                            h.SoLuongTon AS TonHienTai
                        FROM HangHoa h
                        LEFT JOIN ChiTietPhieu ct ON h.MaHang = ct.MaHang
                        LEFT JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                        GROUP BY h.MaHang, h.TenHang, h.SoLuongTon
                        ORDER BY h.MaHang ASC";

                    dt = Database.GetData(sqlFallback,
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgay", denNgayChuan));
                }

                List<BieuDoGdiHelper.ItemThongKe> duLieu = BieuDoGdiHelper.ChuyenDoiDataTable(dt);
                _view.CapNhatBieuDo(duLieu);
            }
            catch (Exception ex)
            {
                _view.HienThongBaoLoi("Lỗi nạp số liệu biểu đồ: " + ex.Message);
            }
        }

        public void InPhieuKho(string maPhieu, bool xemTruoc)
        {
            if (string.IsNullOrEmpty(maPhieu))
            {
                _view.HienThongBao("Vui lòng chọn một phiếu kho trên danh sách để in!");
                return;
            }

            PhieuKhoPrintHelper printer = new PhieuKhoPrintHelper(maPhieu);
            if (xemTruoc)
            {
                printer.XemTruoc(_view.GetWindow());
            }
            else
            {
                printer.InTrucTiep(_view.GetWindow());
            }
        }

        public void XuatCsvLichSu(DataGridView dgv)
        {
            CsvHelperCustom.XuatDataGridViewRaCsv(dgv, "LichSuNhapXuat", _view.GetWindow());
        }

        public void XuatCsvTonKho(DataGridView dgv)
        {
            CsvHelperCustom.XuatDataGridViewRaCsv(dgv, "BaoCaoTonKho", _view.GetWindow());
        }

        public void XuatCsvBieuDo(List<BieuDoGdiHelper.ItemThongKe> duLieu)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Mã hàng");
            dt.Columns.Add("Tên hàng hóa");
            dt.Columns.Add("Tổng nhập", typeof(int));
            dt.Columns.Add("Tổng xuất", typeof(int));
            dt.Columns.Add("Tồn hiện tại", typeof(int));

            if (duLieu != null)
            {
                foreach (var item in duLieu)
                {
                    dt.Rows.Add(item.MaHang, item.TenHang, item.TongNhap, item.TongXuat, item.TonHienTai);
                }
            }

            CsvHelperCustom.XuatDataTableRaCsv(dt, "SoLieuBieuDoNhapXuatTon", _view.GetWindow());
        }

        public void LuuAnhBieuDo(List<BieuDoGdiHelper.ItemThongKe> duLieu, bool laCot)
        {
            BieuDoGdiHelper.LoaiBieuDo loai = laCot ? BieuDoGdiHelper.LoaiBieuDo.CotNhom : BieuDoGdiHelper.LoaiBieuDo.Duong;
            BieuDoGdiHelper.LuuAnhBieuDo(duLieu, loai, _view.GetWindow());
        }
    }
}
