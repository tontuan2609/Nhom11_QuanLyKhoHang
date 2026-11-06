using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using QuanLyKhoHang.Models;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang.Presenters
{
    public class NhaCungCapPresenter
    {
        private readonly INhaCungCapView _view;
        private bool _isEditMode = false;

        public NhaCungCapPresenter(INhaCungCapView view)
        {
            _view = view;
            _view.SetPresenter(this);
            WireEvents();
            LoadAllNhaCungCap();
            _view.SetInputState(false);
        }

        private void WireEvents()
        {
            _view.AddNewEvent += (s, e) => { _isEditMode = false; _view.ClearInputs(); _view.SetInputState(true); };
            _view.EditEvent += (s, e) => { if (!string.IsNullOrEmpty(_view.MaNCC)) { _isEditMode = true; _view.SetInputState(true); } else _view.ShowMessage("Vui lòng chọn một nhà cung cấp để sửa!", true); };
            _view.CancelEvent += (s, e) => { _view.ClearInputs(); _view.SetInputState(false); LoadAllNhaCungCap(); };
            _view.SaveEvent += HandleSave;
            _view.DeleteEvent += HandleDelete;
            _view.SearchEvent += (s, e) => LoadAllNhaCungCap();
        }

        private void LoadAllNhaCungCap()
        {
            var list = new List<NhaCungCap>();
            string query = "SELECT * FROM NhaCungCap WHERE MaNCC LIKE @Key OR TenNCC LIKE @Key";

            using (var conn = DbHelper.GetConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Key", "%" + _view.SearchKeyword.Trim() + "%");
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new NhaCungCap
                        {
                            MaNCC = reader["MaNCC"].ToString(),
                            TenNCC = reader["TenNCC"].ToString(),
                            DienThoai = reader["DienThoai"].ToString(),
                            DiaChi = reader["DiaChi"].ToString()
                        });
                    }
                }
            }
            _view.SetNhaCungCapListBindingSource(list);
        }

        private void HandleSave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_view.MaNCC) || _view.MaNCC.Length > 20)
            {
                _view.ShowMessage("Mã NCC không được trống và không quá 20 ký tự!", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(_view.TenNCC) || _view.TenNCC.Length > 150)
            {
                _view.ShowMessage("Tên nhà cung cấp không được trống và không quá 150 ký tự!", true);
                return;
            }
            if (!Regex.IsMatch(_view.DienThoai.Trim(), @"^\d{9,11}$"))
            {
                _view.ShowMessage("Số điện thoại không hợp lệ! (Phải chứa từ 9 đến 11 ký số)", true);
                return;
            }

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                string query = _isEditMode
                    ? "UPDATE NhaCungCap SET TenNCC=@Ten, DienThoai=@Sdt, DiaChi=@DC WHERE MaNCC=@Ma"
                    : "INSERT INTO NhaCungCap (MaNCC, TenNCC, DienThoai, DiaChi) VALUES (@Ma, @Ten, @Sdt, @DC)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Ma", _view.MaNCC.Trim());
                    cmd.Parameters.AddWithValue("@Ten", _view.TenNCC.Trim());
                    cmd.Parameters.AddWithValue("@Sdt", _view.DienThoai.Trim());
                    cmd.Parameters.AddWithValue("@DC", string.IsNullOrWhiteSpace(_view.DiaChi) ? (object)DBNull.Value : _view.DiaChi.Trim());

                    try
                    {
                        cmd.ExecuteNonQuery();
                        _view.ShowMessage("Lưu dữ liệu Nhà cung cấp thành công!");
                        _view.SetInputState(false);
                        _view.ClearInputs();
                        LoadAllNhaCungCap();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                            _view.ShowMessage("Mã nhà cung cấp này đã tồn tại trong hệ thống!", true);
                        else
                            _view.ShowMessage("Lỗi cơ sở dữ liệu: " + ex.Message, true);
                    }
                }
            }
        }

        private void HandleDelete(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_view.MaNCC))
            {
                _view.ShowMessage("Vui lòng chọn nhà cung cấp cần xóa từ danh sách!", true);
                return;
            }

            using (var conn = DbHelper.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM NhaCungCap WHERE MaNCC = @Ma";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Ma", _view.MaNCC);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        _view.ShowMessage("Xóa nhà cung cấp thành công!");
                        _view.ClearInputs();
                        LoadAllNhaCungCap();
                    }
                    catch (SqlException)
                    {
                        _view.ShowMessage("Không thể xóa! Nhà cung cấp này đã có dữ liệu liên kết trong các Phiếu Kho.", true);
                    }
                }
            }
        }
    }
}