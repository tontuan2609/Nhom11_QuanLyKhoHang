using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang.Presenters
{
    public class PhieuKhoPresenter
    {
        private readonly IPhieuKhoView _view;
        private List<ChiTietPhieu> _listChiTiet = new List<ChiTietPhieu>();
        private Dictionary<string, string> _hangHoaNames = new Dictionary<string, string>();

        public PhieuKhoPresenter(IPhieuKhoView view)
        {
            _view = view;
            WireEvents();
            LoadComboboxData();
        }

        private void WireEvents()
        {
            _view.AddProductToGridEvent += HandleAddProduct;
            _view.RemoveProductFromGridEvent += HandleRemoveProduct;
            _view.SavePhieuKhoEvent += HandleSavePhieuKho;
        }

        private void LoadComboboxData()
        {
            var nccs = new List<KeyValuePair<string, string>>();
            var hhs = new List<KeyValuePair<string, string>>();

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT MaNCC, TenNCC FROM NhaCungCap", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read()) nccs.Add(new KeyValuePair<string, string>(r["MaNCC"].ToString(), r["TenNCC"].ToString()));
                }

                using (var cmd = new SqlCommand("SELECT MaHang, TenHang FROM HangHoa", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string ma = r["MaHang"].ToString();
                        string ten = r["TenHang"].ToString();
                        hhs.Add(new KeyValuePair<string, string>(ma, ten));
                        _hangHoaNames[ma] = ten;
                    }
                }
            }
            _view.BindNhaCungCapCombobox(nccs);
            _view.BindHangHoaCombobox(hhs);
        }

        private int GetCurrentStock(string maHang)
        {
            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT SoLuongTon FROM HangHoa WHERE MaHang = @Ma", conn))
                {
                    cmd.Parameters.AddWithValue("@Ma", maHang);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private void HandleAddProduct(object sender, EventArgs e)
        {
            string maHang = _view.SelectedMaHang;
            if (string.IsNullOrEmpty(maHang)) { _view.ShowMessage("Vui lòng chọn một mặt hàng!", true); return; }

            int soLuongThem = _view.SelectedSoLuong;
            if (soLuongThem <= 0) { _view.ShowMessage("Số lượng phải lớn hơn 0!", true); return; }
            if (_view.SelectedDonGia < 0) { _view.ShowMessage("Đơn giá không được âm!", true); return; }

            if (_view.LoaiPhieu == "Xuất")
            {
                int tonHienTai = GetCurrentStock(maHang);
                var existingItem = _listChiTiet.Find(x => x.MaHang == maHang);
                int daCoTrenLuoi = existingItem != null ? existingItem.SoLuong : 0;

                if (daCoTrenLuoi + soLuongThem > tonHienTai)
                {
                    _view.ShowMessage($"Không đủ hàng tồn! Hiện trong kho chỉ còn: {tonHienTai} mặt hàng. (Đã thêm trong danh mục lập: {daCoTrenLuoi})", true);
                    return;
                }
            }

            var existing = _listChiTiet.Find(x => x.MaHang == maHang);
            if (existing != null) existing.SoLuong += soLuongThem;
            else
            {
                _listChiTiet.Add(new ChiTietPhieu
                {
                    MaHang = maHang,
                    TenHang = _hangHoaNames.ContainsKey(maHang) ? _hangHoaNames[maHang] : maHang,
                    SoLuong = soLuongThem,
                    DonGia = _view.SelectedDonGia
                });
            }

            UpdateTongTien();
            _view.RefreshChiTietGrid(_listChiTiet);
        }

        private void HandleRemoveProduct(object sender, EventArgs e)
        {
            string maHang = _view.SelectedMaHang;
            if (!string.IsNullOrEmpty(maHang))
            {
                _listChiTiet.RemoveAll(x => x.MaHang == maHang);
                UpdateTongTien();
                _view.RefreshChiTietGrid(_listChiTiet);
            }
        }

        private void UpdateTongTien()
        {
            decimal sum = 0;
            foreach (var item in _listChiTiet) sum += item.ThanhTien;
            _view.TongTien = sum;
        }

        private void HandleSavePhieuKho(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_view.MaPhieu) || _view.MaPhieu.Length > 20)
            {
                _view.ShowMessage("Mã phiếu không được để trống và tối đa 20 ký tự!", true);
                return;
            }
            if (_listChiTiet.Count == 0)
            {
                _view.ShowMessage("Vui lòng chọn ít nhất 1 mặt hàng vào danh sách trước khi lưu phiếu!", true);
                return;
            }

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string queryPhieu = @"INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien) 
                                          VALUES (@MaPhieu, @LoaiPhieu, GETDATE(), @MaND, @MaNCC, @GhiChu, @TongTien)";

                    using (var cmd = new SqlCommand(queryPhieu, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@MaPhieu", _view.MaPhieu.Trim());
                        cmd.Parameters.AddWithValue("@LoaiPhieu", _view.LoaiPhieu);
                        string maNguoiDung = PhienDangNhap.DaDangNhap && !string.IsNullOrEmpty(PhienDangNhap.MaNguoiDung)
                            ? PhienDangNhap.MaNguoiDung
                            : "ND01";
                        cmd.Parameters.AddWithValue("@MaND", maNguoiDung);
                        cmd.Parameters.AddWithValue("@MaNCC", string.IsNullOrEmpty(_view.MaNCC) ? (object)DBNull.Value : _view.MaNCC);
                        cmd.Parameters.AddWithValue("@GhiChu", string.IsNullOrWhiteSpace(_view.GhiChu) ? (object)DBNull.Value : _view.GhiChu.Trim());
                        cmd.Parameters.AddWithValue("@TongTien", _view.TongTien);
                        cmd.ExecuteNonQuery();
                    }

                    foreach (var item in _listChiTiet)
                    {
                        if (_view.LoaiPhieu == "Xuất")
                        {
                            int tonThoiDiemLuu = 0;
                            using (var cmdCheck = new SqlCommand("SELECT SoLuongTon FROM HangHoa WHERE MaHang = @Ma", conn, transaction))
                            {
                                cmdCheck.Parameters.AddWithValue("@Ma", item.MaHang);
                                tonThoiDiemLuu = Convert.ToInt32(cmdCheck.ExecuteScalar());
                            }
                            if (item.SoLuong > tonThoiDiemLuu)
                            {
                                throw new Exception($"Mặt hàng [{item.TenHang}] vừa có biến động tồn kho. Số lượng hiện có ({tonThoiDiemLuu}) không đủ đáp ứng yêu cầu xuất ({item.SoLuong})!");
                            }
                        }

                        string queryChiTiet = @"INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia) 
                                                VALUES (@MaPhieu, @MaHang, @SoLuong, @DonGia)";
                        using (var cmd = new SqlCommand(queryChiTiet, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@MaPhieu", _view.MaPhieu.Trim());
                            cmd.Parameters.AddWithValue("@MaHang", item.MaHang);
                            cmd.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmd.Parameters.AddWithValue("@DonGia", item.DonGia);
                            cmd.ExecuteNonQuery();
                        }

                        string queryUpdateKho = _view.LoaiPhieu == "Nhập"
                            ? "UPDATE HangHoa SET SoLuongTon = SoLuongTon + @SL WHERE MaHang = @MaHang"
                            : "UPDATE HangHoa SET SoLuongTon = SoLuongTon - @SL WHERE MaHang = @MaHang";

                        using (var cmdUpdate = new SqlCommand(queryUpdateKho, conn, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@SL", item.SoLuong);
                            cmdUpdate.Parameters.AddWithValue("@MaHang", item.MaHang);
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    _view.ShowMessage($"Lập phiếu {_view.LoaiPhieu} kho thành công! Cơ sở dữ liệu và kho hàng đã được đồng bộ.");

                    _listChiTiet.Clear();
                    _view.ClearForm();
                    _view.RefreshChiTietGrid(_listChiTiet);
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    transaction.Rollback();
                    _view.ShowMessage("Lỗi trùng khóa: Mã phiếu kho này đã tồn tại trong CSDL!", true);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _view.ShowMessage("Hành động bị hủy bỏ và phục hồi trạng thái cũ (Rollback). Lỗi: " + ex.Message, true);
                }
            }
        }
    }
}