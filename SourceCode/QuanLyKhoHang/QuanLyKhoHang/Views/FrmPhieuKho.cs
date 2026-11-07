using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang.Views
{
    public partial class FrmPhieuKho : Form, IPhieuKhoView
    {
        private TextBox txtMaPhieu, txtGhiChu, txtTongTien;
        private ComboBox cbLoaiPhieu, cbNhaCungCap, cbHangHoa;
        private NumericUpDown numSoLuong, numDonGia;
        private Button btnAddProduct, btnRemoveProduct, btnSavePhieu;
        private DataGridView dgvChiTiet;
        private BindingSource _bindingSource = new BindingSource();
        private Presenters.PhieuKhoPresenter _presenter;

        public FrmPhieuKho() : this("Nhập")
        {
        }

        public FrmPhieuKho(string loaiPhieuMacDinh)
        {
            InitializeComponentLayout();
            if (!string.IsNullOrEmpty(loaiPhieuMacDinh))
            {
                cbLoaiPhieu.SelectedItem = loaiPhieuMacDinh;
            }
            _presenter = new Presenters.PhieuKhoPresenter(this);
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Nghiệp Vụ Lập Phiếu Nhập / Xuất Kho";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);

            GroupBox gbThongTinChung = new GroupBox { Text = "Thông tin chung phiếu kho", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(15, 15), Size = new Size(905, 120) };
            Label lblMa = new Label { Text = "Mã Phiếu:", Location = new Point(15, 30), Width = 80, Font = new Font("Segoe UI", 9) };
            txtMaPhieu = new TextBox { Location = new Point(100, 27), Width = 150, Font = new Font("Segoe UI", 9) };
            Label lblLoai = new Label { Text = "Loại Phiếu:", Location = new Point(280, 30), Width = 80, Font = new Font("Segoe UI", 9) };
            cbLoaiPhieu = new ComboBox { Location = new Point(365, 27), Width = 120, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cbLoaiPhieu.Items.AddRange(new object[] { "Nhập", "Xuất" });
            cbLoaiPhieu.SelectedIndex = 0;
            Label lblNCC = new Label { Text = "Nhà Cung Cấp:", Location = new Point(510, 30), Width = 100, Font = new Font("Segoe UI", 9) };
            cbNhaCungCap = new ComboBox { Location = new Point(615, 27), Width = 265, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            Label lblGC = new Label { Text = "Ghi Chú:", Location = new Point(15, 75), Width = 80, Font = new Font("Segoe UI", 9) };
            txtGhiChu = new TextBox { Location = new Point(100, 72), Width = 780, Font = new Font("Segoe UI", 9) };
            gbThongTinChung.Controls.AddRange(new Control[] { lblMa, txtMaPhieu, lblLoai, cbLoaiPhieu, lblNCC, cbNhaCungCap, lblGC, txtGhiChu });
            this.Controls.Add(gbThongTinChung);

            GroupBox gbChiTiet = new GroupBox { Text = "Chi tiết mặt hàng hàng hóa", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(44, 62, 80), Location = new Point(15, 145), Size = new Size(905, 90) };
            Label lblHH = new Label { Text = "Chọn sản phẩm:", Location = new Point(15, 35), Width = 100, Font = new Font("Segoe UI", 9) };
            cbHangHoa = new ComboBox { Location = new Point(120, 32), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            Label lblSL = new Label { Text = "Số lượng:", Location = new Point(390, 35), Width = 60, Font = new Font("Segoe UI", 9) };
            numSoLuong = new NumericUpDown { Location = new Point(455, 32), Width = 70, Minimum = 1, Maximum = 10000, Font = new Font("Segoe UI", 9) };
            Label lblDG = new Label { Text = "Đơn giá lập:", Location = new Point(545, 35), Width = 80, Font = new Font("Segoe UI", 9) };
            numDonGia = new NumericUpDown { Location = new Point(630, 32), Width = 110, Minimum = 0, Maximum = 100000000, DecimalPlaces = 0, Font = new Font("Segoe UI", 9) };
            btnAddProduct = new Button { Text = "Thêm hàng", Location = new Point(760, 28), Size = new Size(120, 30), BackColor = Color.FromArgb(0, 102, 102), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            gbChiTiet.Controls.AddRange(new Control[] { lblHH, cbHangHoa, lblSL, numSoLuong, lblDG, numDonGia, btnAddProduct });
            this.Controls.Add(gbChiTiet);

            dgvChiTiet = new DataGridView { Location = new Point(15, 250), Size = new Size(905, 270), BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, RowHeadersVisible = false };
            this.Controls.Add(dgvChiTiet);

            btnRemoveProduct = new Button { Text = "Xóa dòng chọn", Location = new Point(15, 535), Size = new Size(120, 35), BackColor = Color.FromArgb(217, 83, 79), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Label lblTotal = new Label { Text = "TỔNG TIỀN PHIẾU:", Location = new Point(480, 542), Width = 150, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.DarkRed };
            txtTongTien = new TextBox { Location = new Point(635, 538), Width = 140, Font = new Font("Segoe UI", 11, FontStyle.Bold), ReadOnly = true, TextAlign = HorizontalAlignment.Right, Text = "0.00" };
            btnSavePhieu = new Button { Text = "LƯU PHIẾU KHO", Location = new Point(790, 532), Size = new Size(130, 42), BackColor = Color.FromArgb(92, 184, 92), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            this.Controls.AddRange(new Control[] { btnRemoveProduct, lblTotal, txtTongTien, btnSavePhieu });

            btnAddProduct.Click += (s, e) => AddProductToGridEvent?.Invoke(s, e);
            btnRemoveProduct.Click += (s, e) => RemoveProductFromGridEvent?.Invoke(s, e);
            btnSavePhieu.Click += (s, e) => SavePhieuKhoEvent?.Invoke(s, e);
        }

        public string MaPhieu { get => txtMaPhieu.Text; set => txtMaPhieu.Text = value; }
        public string LoaiPhieu { get => cbLoaiPhieu.SelectedItem?.ToString() ?? "Nhập"; set => cbLoaiPhieu.SelectedItem = value; }
        public string MaNCC { get => cbNhaCungCap.SelectedValue as string; set => cbNhaCungCap.SelectedValue = value; }
        public string GhiChu { get => txtGhiChu.Text; set => txtGhiChu.Text = value; }
        public decimal TongTien { get => decimal.TryParse(txtTongTien.Text, out decimal val) ? val : 0; set => txtTongTien.Text = value.ToString("N2"); }

        public string SelectedMaHang => cbHangHoa.SelectedValue is string str ? str : null;
        public int SelectedSoLuong => (int)numSoLuong.Value;
        public decimal SelectedDonGia => numDonGia.Value;

        public event EventHandler AddProductToGridEvent;
        public event EventHandler RemoveProductFromGridEvent;
        public event EventHandler SavePhieuKhoEvent;

        public void BindNhaCungCapCombobox(List<KeyValuePair<string, string>> nccData) { cbNhaCungCap.DataSource = nccData; cbNhaCungCap.DisplayMember = "Value"; cbNhaCungCap.ValueMember = "Key"; }
        public void BindHangHoaCombobox(List<KeyValuePair<string, string>> hangHoaData) { cbHangHoa.DataSource = hangHoaData; cbHangHoa.DisplayMember = "Value"; cbHangHoa.ValueMember = "Key"; }

        public void RefreshChiTietGrid(List<ChiTietPhieu> list)
        {
            _bindingSource.DataSource = null; _bindingSource.DataSource = list; dgvChiTiet.DataSource = _bindingSource;
            if (dgvChiTiet.Columns["MaPhieu"] != null) dgvChiTiet.Columns["MaPhieu"].Visible = false;
        }

        public void ShowMessage(string message, bool isError = false) => MessageBox.Show(message, isError ? "Cảnh báo dữ liệu" : "Thông báo", MessageBoxButtons.OK, isError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        public void ClearForm() { txtMaPhieu.Clear(); txtGhiChu.Clear(); txtTongTien.Text = "0.00"; numSoLuong.Value = 1; numDonGia.Value = 0; }
    }
}