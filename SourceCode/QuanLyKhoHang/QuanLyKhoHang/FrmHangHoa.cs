using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmHangHoa : Form, QuanLyKhoHang.Views.IHangHoaView
    {
        private TextBox txtMaHang;
        private TextBox txtTenHang;
        private ComboBox cboLoai;
        private TextBox txtDonViTinh;
        private NumericUpDown nudDonGiaNhap;
        private NumericUpDown nudTon;
        private NumericUpDown nudTonToiThieu;
        private PictureBox picHinhAnh;
        private Button btnChonAnh;
        private Button btnBoAnh;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLamMoi;
        private TextBox txtTimKiem;
        private ComboBox cboLoc;
        private CheckBox chkSapHet;
        private DataGridView dgv;
        private ErrorProvider errorProvider;
        private string duongDanAnh;
        private bool choPhep = true;
        private readonly QuanLyKhoHang.Presenters.HangHoaPresenter presenter;

        public event EventHandler ThemClicked, SuaClicked, XoaClicked, LamMoiClicked, BoAnhClicked, LocThayDoiClicked;

        public FrmHangHoa()
        {
            KhoiTaoGiaoDien();
            PhanQuyen();
            presenter = new QuanLyKhoHang.Presenters.HangHoaPresenter(this);
        }

        private void KhoiTaoGiaoDien()
        {
            Text = "Quản lý hàng hóa";
            ClientSize = new Size(1000, 600);
            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            Panel pnl = new Panel { Dock = DockStyle.Top, Height = 255 };

            UiHelper.TaoNhan(pnl, "Mã hàng:", 20, 15);
            txtMaHang = UiHelper.TaoTextBox(pnl, 130, 15, 200, 20);
            UiHelper.TaoNhan(pnl, "Tên hàng:", 370, 15);
            txtTenHang = UiHelper.TaoTextBox(pnl, 490, 15, 270, 150);

            UiHelper.TaoNhan(pnl, "Loại hàng:", 20, 50);
            cboLoai = new ComboBox { Left = 130, Top = 50, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cboLoai);
            UiHelper.TaoNhan(pnl, "Đơn vị tính:", 370, 50);
            txtDonViTinh = UiHelper.TaoTextBox(pnl, 490, 50, 150, 30);

            UiHelper.TaoNhan(pnl, "Đơn giá nhập:", 20, 85);
            nudDonGiaNhap = UiHelper.TaoSo(pnl, 130, 85, 200, 100000000000m);
            UiHelper.TaoNhan(pnl, "Mức tồn tối thiểu:", 370, 85);
            nudTonToiThieu = UiHelper.TaoSo(pnl, 490, 85, 150, 100000000m);
            nudTonToiThieu.Value = 10;

            UiHelper.TaoNhan(pnl, "Số lượng tồn:", 20, 120);
            nudTon = UiHelper.TaoSo(pnl, 130, 120, 200, 100000000m);

            picHinhAnh = new PictureBox
            {
                Left = 790,
                Top = 15,
                Width = 180,
                Height = 135,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnl.Controls.Add(picHinhAnh);
            btnChonAnh = UiHelper.TaoNut(pnl, "Chọn ảnh...", 790, 158, 85);
            btnBoAnh = UiHelper.TaoNut(pnl, "Bỏ ảnh", 885, 158, 85);

            btnThem = UiHelper.TaoNut(pnl, "Thêm", 130, 165, 90);
            btnSua = UiHelper.TaoNut(pnl, "Sửa", 230, 165, 90);
            btnXoa = UiHelper.TaoNut(pnl, "Xóa", 330, 165, 90);
            btnLamMoi = UiHelper.TaoNut(pnl, "Làm mới", 430, 165, 90);

            UiHelper.TaoNhan(pnl, "Tìm kiếm:", 20, 210);
            txtTimKiem = UiHelper.TaoTextBox(pnl, 130, 210, 200, 100);
            UiHelper.TaoNhan(pnl, "Lọc theo loại:", 370, 210);
            cboLoc = new ComboBox { Left = 490, Top = 210, Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
            pnl.Controls.Add(cboLoc);
            chkSapHet = new CheckBox { Text = "Chỉ hiện hàng sắp hết", Left = 720, Top = 212, AutoSize = true };
            pnl.Controls.Add(chkSapHet);

            dgv = UiHelper.TaoLuoi();
            Controls.Add(dgv);
            Controls.Add(pnl);

            btnThem.Click += (s, e) => ThemClicked?.Invoke(this, EventArgs.Empty);
            btnSua.Click += (s, e) => SuaClicked?.Invoke(this, EventArgs.Empty);
            btnXoa.Click += (s, e) => XoaClicked?.Invoke(this, EventArgs.Empty);
            btnLamMoi.Click += (s, e) => LamMoiClicked?.Invoke(this, EventArgs.Empty);
            btnChonAnh.Click += BtnChonAnh_Click;
            btnBoAnh.Click += (s, e) => BoAnhClicked?.Invoke(this, EventArgs.Empty);
            txtTimKiem.TextChanged += LocThayDoi;
            cboLoc.SelectedIndexChanged += LocThayDoi;
            chkSapHet.CheckedChanged += LocThayDoi;
            dgv.SelectionChanged += Dgv_SelectionChanged;
            Shown += (s, e) => DatLaiForm();
        }

        private void PhanQuyen()
        {
            if (!PhienDangNhap.LaQuanLy)
            {
                btnThem.Enabled = false;
                btnSua.Enabled = false;
                btnXoa.Enabled = false;
                btnChonAnh.Enabled = false;
                btnBoAnh.Enabled = false;
                Text += " (chỉ xem)";
            }
        }

        private void LocThayDoi(object sender, EventArgs e)
        {
            if (choPhep) LocThayDoiClicked?.Invoke(this, EventArgs.Empty);
        }

        public void HienThiLoai(DataTable dtLoai)
        {
            bool truoc = choPhep;
            choPhep = false;
            try
            {
                DataTable dtLoc = dtLoai.Copy();
                DataRow tatCa = dtLoc.NewRow();
                tatCa["MaLoai"] = string.Empty;
                tatCa["TenLoai"] = "-- Tất cả --";
                dtLoc.Rows.InsertAt(tatCa, 0);
                cboLoai.DisplayMember = "TenLoai";
                cboLoai.ValueMember = "MaLoai";
                cboLoai.DataSource = dtLoai;
                cboLoai.SelectedIndex = -1;
                cboLoc.DisplayMember = "TenLoai";
                cboLoc.ValueMember = "MaLoai";
                cboLoc.DataSource = dtLoc;
                cboLoc.SelectedIndex = 0;
            }
            finally { choPhep = truoc; }
        }

        public void HienThiDanhSach(DataTable dt)
        {
            bool truoc = choPhep;
            choPhep = false;
            try
            {
                DataView dv = dt.DefaultView;
                dv.RowFilter = chkSapHet.Checked ? "SoLuongTon <= MucTonToiThieu" : string.Empty;
                dgv.DataSource = dv;
                dgv.Columns["MaHang"].HeaderText = "Mã hàng";
                dgv.Columns["TenHang"].HeaderText = "Tên hàng";
                dgv.Columns["MaLoai"].Visible = false;
                dgv.Columns["TenLoai"].HeaderText = "Loại hàng";
                dgv.Columns["DonViTinh"].HeaderText = "ĐVT";
                dgv.Columns["DonGiaNhap"].HeaderText = "Đơn giá nhập";
                dgv.Columns["DonGiaNhap"].DefaultCellStyle.Format = "N0";
                dgv.Columns["DonGiaNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgv.Columns["SoLuongTon"].HeaderText = "Tồn";
                dgv.Columns["MucTonToiThieu"].HeaderText = "Tồn tối thiểu";
                dgv.Columns["HinhAnh"].Visible = false;
                dgv.Columns["TrangThaiTon"].HeaderText = "Trạng thái";
                foreach (DataGridViewRow dong in dgv.Rows)
                {
                    int ton = Convert.ToInt32(dong.Cells["SoLuongTon"].Value);
                    int toiThieu = Convert.ToInt32(dong.Cells["MucTonToiThieu"].Value);
                    if (ton <= toiThieu)
                    {
                        dong.DefaultCellStyle.BackColor = Color.MistyRose;
                        dong.DefaultCellStyle.ForeColor = Color.Firebrick;
                    }
                }
                dgv.CurrentCell = null;
                dgv.ClearSelection();
            }
            finally { choPhep = truoc; }
        }

        private void HienAnh(string duongDan)
        {
            if (picHinhAnh.Image != null)
            {
                picHinhAnh.Image.Dispose();
                picHinhAnh.Image = null;
            }

            duongDanAnh = string.IsNullOrWhiteSpace(duongDan) ? null : duongDan;
            if (duongDanAnh == null || !File.Exists(duongDanAnh))
            {
                return;
            }

            try
            {
                using (FileStream fs = new FileStream(duongDanAnh, FileMode.Open, FileAccess.Read))
                using (Image anh = Image.FromStream(fs))
                {
                    picHinhAnh.Image = new Bitmap(anh);
                }
            }
            catch (Exception)
            {
                picHinhAnh.Image = null;
            }
        }

        private void BtnChonAnh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog hopThoai = new OpenFileDialog())
            {
                hopThoai.Title = "Chọn hình ảnh hàng hóa";
                hopThoai.Filter = "Hình ảnh|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Tất cả tệp|*.*";
                if (hopThoai.ShowDialog() == DialogResult.OK)
                {
                    HienAnh(hopThoai.FileName);
                }
            }
        }

        public void DatLaiForm()
        {
            txtMaHang.Clear();
            txtTenHang.Clear();
            txtDonViTinh.Clear();
            cboLoai.SelectedIndex = -1;
            nudDonGiaNhap.Value = 0;
            nudTon.Value = 0;
            nudTonToiThieu.Value = 10;
            HienAnh(null);
            txtMaHang.ReadOnly = false;
            nudTon.Enabled = true;
            errorProvider.Clear();
            dgv.ClearSelection();
            txtMaHang.Focus();
        }

        private static string Chuoi(DataGridViewRow dong, string cot)
        {
            return Convert.ToString(dong.Cells[cot].Value);
        }

        public void ChonDong(string ma)
        {
            foreach (DataGridViewRow dong in dgv.Rows)
            {
                if (Chuoi(dong, "MaHang") == ma)
                {
                    dgv.CurrentCell = dong.Cells["MaHang"];
                    dong.Selected = true;
                    return;
                }
            }
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (!choPhep || dgv.SelectedRows.Count == 0)
            {
                return;
            }

            DataGridViewRow dong = dgv.SelectedRows[0];
            txtMaHang.Text = Chuoi(dong, "MaHang");
            txtTenHang.Text = Chuoi(dong, "TenHang");
            cboLoai.SelectedValue = Chuoi(dong, "MaLoai");
            txtDonViTinh.Text = Chuoi(dong, "DonViTinh");
            UiHelper.GanSo(nudDonGiaNhap, dong.Cells["DonGiaNhap"].Value);
            UiHelper.GanSo(nudTon, dong.Cells["SoLuongTon"].Value);
            UiHelper.GanSo(nudTonToiThieu, dong.Cells["MucTonToiThieu"].Value);
            HienAnh(Chuoi(dong, "HinhAnh"));
            txtMaHang.ReadOnly = true;
            nudTon.Enabled = false;
            errorProvider.Clear();
        }

        public string MaHang { get { return txtMaHang.Text; } }
        public string TenHang { get { return txtTenHang.Text; } }
        public string MaLoai { get { return cboLoai.SelectedValue as string; } }
        public string DonViTinh { get { return txtDonViTinh.Text; } }
        public decimal DonGiaNhap { get { return nudDonGiaNhap.Value; } }
        public int SoLuongTon { get { return (int)nudTon.Value; } }
        public int MucTonToiThieu { get { return (int)nudTonToiThieu.Value; } }
        public string DuongDanAnh { get { return duongDanAnh; } }
        public string TuKhoa { get { return txtTimKiem.Text; } }
        public string MaLoaiLoc { get { return cboLoc.SelectedValue as string ?? string.Empty; } }
        public bool DangChonHangHoa { get { return txtMaHang.ReadOnly; } }
        public void BoAnh() { HienAnh(null); }
        public void XoaLoiNhapLieu() { errorProvider.Clear(); }
        public void BaoLoiMaHang(string s) { errorProvider.SetError(txtMaHang, s); }
        public void BaoLoiTenHang(string s) { errorProvider.SetError(txtTenHang, s); }
        public void BaoLoiLoai(string s) { errorProvider.SetError(cboLoai, s); }
        public void BaoLoiDonViTinh(string s) { errorProvider.SetError(txtDonViTinh, s); }
        public void BaoLoiDonGiaNhap(string s) { errorProvider.SetError(nudDonGiaNhap, s); }
        public void HienThongBao(string s) { UiHelper.HienThongBao(s); }
        public void HienLoi(string s) { UiHelper.HienLoi(s); }
        public bool XacNhan(string s) { return UiHelper.XacNhan(s); }
    }
}
