using System;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhoHang.Views;
using QuanLyKhoHang.Presenters;

namespace QuanLyKhoHang
{
    public class FrmLogin : Form, ILoginView
    {
        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private CheckBox chkHienMatKhau;
        private Button btnDangNhap;
        private Button btnThoat;
        private ErrorProvider errorProvider;
        private readonly LoginPresenter presenter;

        public event EventHandler DangNhapClicked;

        public string TenDangNhap { get { return txtTenDangNhap.Text; } }
        public string MatKhau { get { return txtMatKhau.Text; } }

        public FrmLogin()
        {
            KhoiTaoGiaoDien();
            presenter = new LoginPresenter(this);
        }

        private void KhoiTaoGiaoDien()
        {
            Text = "Đăng nhập hệ thống";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(380, 250);

            errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };

            Label lblTieuDe = new Label
            {
                Text = "QUẢN LÝ KHO HÀNG",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.SteelBlue,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0,
                Top = 20,
                Width = 380,
                Height = 40
            };
            Controls.Add(lblTieuDe);

            UiHelper.TaoNhan(this, "Tên đăng nhập:", 30, 80);
            txtTenDangNhap = UiHelper.TaoTextBox(this, 140, 80, 200, 50);

            UiHelper.TaoNhan(this, "Mật khẩu:", 30, 120);
            txtMatKhau = UiHelper.TaoTextBox(this, 140, 120, 200, 100);
            txtMatKhau.UseSystemPasswordChar = true;

            chkHienMatKhau = new CheckBox { Text = "Hiện mật khẩu", Left = 140, Top = 152, AutoSize = true };
            Controls.Add(chkHienMatKhau);

            btnDangNhap = UiHelper.TaoNut(this, "Đăng nhập", 140, 190, 95);
            btnThoat = UiHelper.TaoNut(this, "Thoát", 245, 190, 95);
            btnThoat.DialogResult = DialogResult.Cancel;

            AcceptButton = btnDangNhap;
            CancelButton = btnThoat;

            btnDangNhap.Click += (s, e) => DangNhapClicked?.Invoke(this, EventArgs.Empty);
            chkHienMatKhau.CheckedChanged += (s, e) => txtMatKhau.UseSystemPasswordChar = !chkHienMatKhau.Checked;
        }

        public void XoaLoiNhapLieu() { errorProvider.Clear(); }
        public void BaoLoiTenDangNhap(string noiDung) { errorProvider.SetError(txtTenDangNhap, noiDung); }
        public void BaoLoiMatKhau(string noiDung) { errorProvider.SetError(txtMatKhau, noiDung); }
        public void HienLoi(string noiDung) { UiHelper.HienLoi(noiDung); }
        public void XoaMatKhau() { txtMatKhau.Clear(); txtMatKhau.Focus(); }
        public void DangNhapThanhCong() { DialogResult = DialogResult.OK; }
    }
}
