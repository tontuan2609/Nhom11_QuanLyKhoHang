using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmLogin : Form
    {
        private TextBox txtTenDangNhap;
        private TextBox txtMatKhau;
        private CheckBox chkHienMatKhau;
        private Button btnDangNhap;
        private Button btnThoat;
        private ErrorProvider errorProvider;

        public FrmLogin()
        {
            KhoiTaoGiaoDien();
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

            btnDangNhap.Click += BtnDangNhap_Click;
            chkHienMatKhau.CheckedChanged += (s, e) => txtMatKhau.UseSystemPasswordChar = !chkHienMatKhau.Checked;
        }

        private void BtnDangNhap_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            string tenDangNhap = txtTenDangNhap.Text.Trim();
            string matKhau = txtMatKhau.Text;

            bool hopLe = true;
            if (tenDangNhap.Length == 0)
            {
                errorProvider.SetError(txtTenDangNhap, "Vui lòng nhập tên đăng nhập");
                hopLe = false;
            }
            if (matKhau.Length == 0)
            {
                errorProvider.SetError(txtMatKhau, "Vui lòng nhập mật khẩu");
                hopLe = false;
            }
            if (!hopLe)
            {
                return;
            }

            try
            {
                DataTable dt = Database.GetData(
                    "SELECT MaNguoiDung, TenDangNhap, HoTen, VaiTro FROM NguoiDung " +
                    "WHERE TenDangNhap = @ten AND MatKhau = @mk COLLATE Latin1_General_CS_AS AND TrangThai = 1",
                    Database.Param("@ten", tenDangNhap),
                    Database.Param("@mk", matKhau));

                if (dt.Rows.Count == 0)
                {
                    UiHelper.HienLoi("Sai tên đăng nhập hoặc mật khẩu, hoặc tài khoản đã bị khóa.");
                    txtMatKhau.Clear();
                    txtMatKhau.Focus();
                    return;
                }

                DataRow dong = dt.Rows[0];
                PhienDangNhap.DangNhap(
                    Convert.ToString(dong["MaNguoiDung"]),
                    Convert.ToString(dong["TenDangNhap"]),
                    Convert.ToString(dong["HoTen"]),
                    Convert.ToString(dong["VaiTro"]));

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                UiHelper.HienLoi("Không kết nối được cơ sở dữ liệu. Hãy kiểm tra SQL Server đang chạy và chuỗi kết nối trong App.config.\n\nChi tiết: " + ex.Message);
            }
        }
    }
}
