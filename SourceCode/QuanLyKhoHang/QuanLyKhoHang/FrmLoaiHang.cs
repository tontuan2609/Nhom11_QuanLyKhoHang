using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    using QuanLyKhoHang.Views;
    using QuanLyKhoHang.Presenters;
    public class FrmLoaiHang : Form, ILoaiHangView
    {
        private TextBox txtMaLoai;
        private TextBox txtTenLoai;
        private TextBox txtMoTa;
        private TextBox txtTimKiem;
        private Button btnThem;
        private Button btnSua;
        private Button btnXoa;
        private Button btnLamMoi;
        private DataGridView dgv;
        private ErrorProvider errorProvider;
        private bool choPhep = true;
        private readonly LoaiHangPresenter presenter;

        public event EventHandler ThemClicked;
        public event EventHandler SuaClicked;
        public event EventHandler XoaClicked;
        public event EventHandler LamMoiClicked;
        public event EventHandler TimKiemChanged;


        public string MaLoai
        {
            get { return txtMaLoai.Text; }
            set { txtMaLoai.Text = value; }
        }

        public string TenLoai
        {
            get { return txtTenLoai.Text; }
            set { txtTenLoai.Text = value; }
        }

        public string MoTa
        {
            get { return txtMoTa.Text; }
            set { txtMoTa.Text = value; }
        }

        public string TuKhoa
        {
            get { return txtTimKiem.Text; }
        }

        public bool DangChonLoaiHang
        {
            get { return txtMaLoai.ReadOnly; }
        }



        public FrmLoaiHang()
        {
            KhoiTaoGiaoDien();
            PhanQuyen();

            presenter = new LoaiHangPresenter(this);
        }


        private void KhoiTaoGiaoDien()
        {
            Text = "Quản lý loại hàng";
            ClientSize = new Size(820, 520);
            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            Panel pnl = new Panel { Dock = DockStyle.Top, Height = 175 };

            UiHelper.TaoNhan(pnl, "Mã loại:", 20, 15);
            txtMaLoai = UiHelper.TaoTextBox(pnl, 110, 15, 160, 20);
            UiHelper.TaoNhan(pnl, "Tên loại:", 20, 50);
            txtTenLoai = UiHelper.TaoTextBox(pnl, 110, 50, 360, 100);
            UiHelper.TaoNhan(pnl, "Mô tả:", 20, 85);
            txtMoTa = UiHelper.TaoTextBox(pnl, 110, 85, 360, 255);

            btnThem = UiHelper.TaoNut(pnl, "Thêm", 110, 125, 90);
            btnSua = UiHelper.TaoNut(pnl, "Sửa", 210, 125, 90);
            btnXoa = UiHelper.TaoNut(pnl, "Xóa", 310, 125, 90);
            btnLamMoi = UiHelper.TaoNut(pnl, "Làm mới", 410, 125, 90);

            UiHelper.TaoNhan(pnl, "Tìm kiếm:", 540, 15);
            txtTimKiem = UiHelper.TaoTextBox(pnl, 620, 15, 180, 50);

            dgv = UiHelper.TaoLuoi();
            Controls.Add(dgv);
            Controls.Add(pnl);

            btnThem.Click += (s, e) =>
            {
                ThemClicked?.Invoke(this, EventArgs.Empty);
            };

            btnSua.Click += (s, e) =>
            {
                SuaClicked?.Invoke(this, EventArgs.Empty);
            };

            btnXoa.Click += (s, e) =>
            {
                XoaClicked?.Invoke(this, EventArgs.Empty);
            };

            btnLamMoi.Click += (s, e) =>
            {
                LamMoiClicked?.Invoke(this, EventArgs.Empty);
            };

            txtTimKiem.TextChanged += (s, e) =>
            {
                if (choPhep)
                {
                    TimKiemChanged?.Invoke(this, EventArgs.Empty);
                }
            };

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
                Text += " (chỉ xem)";
            }
        }

        public void DatLaiForm()
        {
            txtMaLoai.Clear();
            txtTenLoai.Clear();
            txtMoTa.Clear();
            txtMaLoai.ReadOnly = false;
            errorProvider.Clear();
            dgv.ClearSelection();
            txtMaLoai.Focus();
        }

        public void ChonDong(string ma)
        {
            foreach (DataGridViewRow dong in dgv.Rows)
            {
                if (Convert.ToString(dong.Cells["MaLoai"].Value) == ma)
                {
                    dgv.CurrentCell = dong.Cells["MaLoai"];
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
            txtMaLoai.Text = Convert.ToString(dong.Cells["MaLoai"].Value);
            txtTenLoai.Text = Convert.ToString(dong.Cells["TenLoai"].Value);
            txtMoTa.Text = Convert.ToString(dong.Cells["MoTa"].Value);
            txtMaLoai.ReadOnly = true;
            errorProvider.Clear();
        }

        public void HienThiDanhSach(DataTable duLieu)
        {
            bool truoc = choPhep;
            choPhep = false;

            try
            {
                dgv.DataSource = duLieu;

                dgv.Columns["MaLoai"].HeaderText = "Mã loại";
                dgv.Columns["TenLoai"].HeaderText = "Tên loại";
                dgv.Columns["MoTa"].HeaderText = "Mô tả";
                dgv.Columns["SoHangHoa"].HeaderText = "Số hàng hóa";

                dgv.CurrentCell = null;
                dgv.ClearSelection();
            }
            finally
            {
                choPhep = truoc;
            }
        }

        public void HienThongBao(string noiDung)
        {
            UiHelper.HienThongBao(noiDung);
        }
        public void HienLoi(string noiDung)
        {
            UiHelper.HienLoi(noiDung);
        }

        public bool XacNhan(string noiDung)
        {
            return UiHelper.XacNhan(noiDung);
        }

        public void BaoLoiMaLoai(string noiDung)
        {
            errorProvider.SetError(txtMaLoai, noiDung);
        }

        public void BaoLoiTenLoai(string noiDung)
        {
            errorProvider.SetError(txtTenLoai, noiDung);
        }

        public void XoaLoiNhapLieu()
        {
            errorProvider.Clear();
        }

    }
}
