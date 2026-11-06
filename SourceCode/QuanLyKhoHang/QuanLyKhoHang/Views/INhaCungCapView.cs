using System;
using System.Collections.Generic;
using QuanLyKhoHang.Models;

namespace QuanLyKhoHang.Views
{
    public interface INhaCungCapView
    {
        string MaNCC { get; set; }
        string TenNCC { get; set; }
        string DienThoai { get; set; }
        string DiaChi { get; set; }
        string SearchKeyword { get; set; }

        event EventHandler AddNewEvent;
        event EventHandler EditEvent;
        event EventHandler DeleteEvent;
        event EventHandler SaveEvent;
        event EventHandler CancelEvent;
        event EventHandler SearchEvent;

        void SetNhaCungCapListBindingSource(List<NhaCungCap> list);
        void SetPresenter(Presenters.NhaCungCapPresenter presenter);
        void ShowMessage(string message, bool isError = false);
        void ClearInputs();
        void SetInputState(bool isEditing);
    }
}