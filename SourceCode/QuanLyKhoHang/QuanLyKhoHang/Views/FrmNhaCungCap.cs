using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhoHang.Models;

namespace QuanLyKhoHang.Views
{
    public partial class FrmNhaCungCap : Form, INhaCungCapView
    {
        private TextBox txtMaNCC, txtTenNCC, txtDienThoai, txtDiaChi, txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnSave, btnCancel, btnSearch;
        private DataGridView dgvNhaCungCap;
        private BindingSource _bindingSource = new BindingSource();
        private Presenters.NhaCungCapPresenter _presenter;

        public FrmNhaCungCap()
        {
            InitializeComponentLayout();
            _presenter = new Presenters.NhaCungCapPresenter(this);
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Quản Lý Danh Mục Nhà Cung Cấp";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(850, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Label lblTitle = new Label { Text = "DANH MỤC NHÀ CUNG CẤP", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 102, 102), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleCenter };
            this.Controls.Add(lblTitle);

            Panel pnlInputs = new Panel { Width = 320, Dock = DockStyle.Left, Padding = new Padding(15) };
            this.Controls.Add(pnlInputs);

            int yOffset = 20;
            txtMaNCC = CreateInputField(pnlInputs, "Mã Nhà Cung Cấp:", ref yOffset);
            txtTenNCC = CreateInputField(pnlInputs, "Tên Nhà Cung Cấp:", ref yOffset);
            txtDienThoai = CreateInputField(pnlInputs, "Số Điện Thoại:", ref yOffset);
            txtDiaChi = CreateInputField(pnlInputs, "Địa Chỉ:", ref yOffset, true);

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel { Location = new Point(15, yOffset + 10), Size = new Size(290, 120), FlowDirection = FlowDirection.LeftToRight };
            pnlInputs.Controls.Add(pnlButtons);

            btnAdd = CreateButton(pnlButtons, "Thêm", Color.FromArgb(0, 122, 255));
            btnEdit = CreateButton(pnlButtons, "Sửa", Color.FromArgb(240, 173, 78));
            btnDelete = CreateButton(pnlButtons, "Xóa", Color.FromArgb(217, 83, 79));
            btnSave = CreateButton(pnlButtons, "Lưu", Color.FromArgb(92, 184, 92));
            btnCancel = CreateButton(pnlButtons, "Hủy", Color.FromArgb(144, 144, 144));

            Panel pnlGrid = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 15, 15, 15) };
            this.Controls.Add(pnlGrid);

            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 45 };
            txtSearch = new TextBox { Location = new Point(0, 5), Width = 350, Font = new Font("Segoe UI", 10) };
            btnSearch = new Button { Text = "Tìm kiếm", Location = new Point(360, 4), Size = new Size(90, 28), BackColor = Color.FromArgb(74, 85, 104), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            pnlSearch.Controls.AddRange(new Control[] { txtSearch, btnSearch });
            pnlGrid.Controls.Add(pnlSearch);

            dgvNhaCungCap = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false };
            dgvNhaCungCap.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvNhaCungCap.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 102);
            dgvNhaCungCap.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvNhaCungCap.EnableHeadersVisualStyles = false;
            dgvNhaCungCap.SelectionChanged += DgvNhaCungCap_SelectionChanged;

            pnlGrid.Controls.Add(dgvNhaCungCap);
            dgvNhaCungCap.BringToFront();

            btnAdd.Click += (s, e) => AddNewEvent?.Invoke(s, e);
            btnEdit.Click += (s, e) => EditEvent?.Invoke(s, e);
            btnDelete.Click += (s, e) => DeleteEvent?.Invoke(s, e);
            btnSave.Click += (s, e) => SaveEvent?.Invoke(s, e);
            btnCancel.Click += (s, e) => CancelEvent?.Invoke(s, e);
            btnSearch.Click += (s, e) => SearchEvent?.Invoke(s, e);
        }

        private TextBox CreateInputField(Panel panel, string labelText, ref int yOffset, bool isMultiLine = false)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(15, yOffset), Width = 280, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(45, 55, 72) };
            TextBox txt = new TextBox { Location = new Point(15, yOffset + 20), Width = 270, Font = new Font("Segoe UI", 10) };
            if (isMultiLine) { txt.Multiline = true; txt.Height = 60; yOffset += 40; }
            panel.Controls.AddRange(new Control[] { lbl, txt });
            yOffset += 55;
            return txt;
        }

        private Button CreateButton(FlowLayoutPanel panel, string text, Color color)
        {
            Button btn = new Button { Text = text, Width = 85, Height = 32, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            panel.Controls.Add(btn);
            return btn;
        }

        private void DgvNhaCungCap_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNhaCungCap.CurrentRow != null)
            {
                var ncc = (NhaCungCap)dgvNhaCungCap.CurrentRow.DataBoundItem;
                MaNCC = ncc.MaNCC; TenNCC = ncc.TenNCC; DienThoai = ncc.DienThoai; DiaChi = ncc.DiaChi;
            }
        }

        public string MaNCC { get => txtMaNCC.Text; set => txtMaNCC.Text = value; }
        public string TenNCC { get => txtTenNCC.Text; set => txtTenNCC.Text = value; }
        public string DienThoai { get => txtDienThoai.Text; set => txtDienThoai.Text = value; }
        public string DiaChi { get => txtDiaChi.Text; set => txtDiaChi.Text = value; }
        public string SearchKeyword { get => txtSearch.Text; set => txtSearch.Text = value; }

        public event EventHandler AddNewEvent;
        public event EventHandler EditEvent;
        public event EventHandler DeleteEvent;
        public event EventHandler SaveEvent;
        public event EventHandler CancelEvent;
        public event EventHandler SearchEvent;

        public void SetNhaCungCapListBindingSource(List<NhaCungCap> list) { _bindingSource.DataSource = list; dgvNhaCungCap.DataSource = _bindingSource; }
        public void SetPresenter(Presenters.NhaCungCapPresenter presenter) { _presenter = presenter; }
        public void ShowMessage(string message, bool isError = false) => MessageBox.Show(message, isError ? "Lỗi" : "Thông báo", MessageBoxButtons.OK, isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        public void ClearInputs() { txtMaNCC.Clear(); txtTenNCC.Clear(); txtDienThoai.Clear(); txtDiaChi.Clear(); }
        public void SetInputState(bool isEditing)
        {
            txtMaNCC.Enabled = isEditing && !btnEdit.Enabled; txtTenNCC.Enabled = isEditing; txtDienThoai.Enabled = isEditing; txtDiaChi.Enabled = isEditing;
            btnSave.Enabled = isEditing; btnCancel.Enabled = isEditing; btnAdd.Enabled = !isEditing; btnEdit.Enabled = !isEditing; btnDelete.Enabled = !isEditing;
        }
    }
}