using System;
using System.Collections.Generic;

namespace QuanLyKhoHang.Views
{
    public interface IPhieuKhoView
    {
        string MaPhieu { get; set; }
        string LoaiPhieu { get; set; }
        string MaNCC { get; set; }
        string GhiChu { get; set; }
        decimal TongTien { get; set; }

        string SelectedMaHang { get; }
        int SelectedSoLuong { get; }
        decimal SelectedDonGia { get; }

        event EventHandler AddProductToGridEvent;
        event EventHandler RemoveProductFromGridEvent;
        event EventHandler SavePhieuKhoEvent;

        void BindNhaCungCapCombobox(List<KeyValuePair<string, string>> nccData);
        void BindHangHoaCombobox(List<KeyValuePair<string, string>> hangHoaData);
        void RefreshChiTietGrid(List<ChiTietPhieu> list);
        void ShowMessage(string message, bool isError = false);
        void ClearForm();
    }
}