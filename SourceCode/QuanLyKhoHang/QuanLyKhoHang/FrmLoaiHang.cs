using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmLoaiHang : Form
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

        public FrmLoaiHang()
        {
            KhoiTaoGiaoDien();
            TaiDuLieu();
            PhanQuyen();
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

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => DatLaiForm();
            txtTimKiem.TextChanged += (s, e) =>
            {
                if (choPhep)
                {
                    TaiDuLieu();
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

        private void TaiDuLieu()
        {
            bool truoc = choPhep;
            choPhep = false;
            try
            {
                string tuKhoa = "%" + txtTimKiem.Text.Trim() + "%";
                dgv.DataSource = Database.GetData(
                    "SELECT l.MaLoai, l.TenLoai, l.MoTa, COUNT(h.MaHang) AS SoHangHoa " +
                    "FROM LoaiHang l LEFT JOIN HangHoa h ON h.MaLoai = l.MaLoai " +
                    "WHERE l.MaLoai LIKE @k OR l.TenLoai LIKE @k " +
                    "GROUP BY l.MaLoai, l.TenLoai, l.MoTa ORDER BY l.MaLoai",
                    Database.Param("@k", tuKhoa));

                dgv.Columns["MaLoai"].HeaderText = "Mã loại";
                dgv.Columns["TenLoai"].HeaderText = "Tên loại";
                dgv.Columns["MoTa"].HeaderText = "Mô tả";
                dgv.Columns["SoHangHoa"].HeaderText = "Số hàng hóa";

                dgv.CurrentCell = null;
                dgv.ClearSelection();
            }
            catch (Exception ex)
            {
                UiHelper.HienLoi("Không tải được dữ liệu: " + ex.Message);
            }
            finally
            {
                choPhep = truoc;
            }
        }

        private void DatLaiForm()
        {
            txtMaLoai.Clear();
            txtTenLoai.Clear();
            txtMoTa.Clear();
            txtMaLoai.ReadOnly = false;
            errorProvider.Clear();
            dgv.ClearSelection();
            txtMaLoai.Focus();
        }

        private void ChonDong(string ma)
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

        private bool KiemTra()
        {
            errorProvider.Clear();
            bool hopLe = true;

            string ma = txtMaLoai.Text.Trim();
            if (ma.Length == 0)
            {
                errorProvider.SetError(txtMaLoai, "Vui lòng nhập mã loại");
                hopLe = false;
            }
            else if (!Regex.IsMatch(ma, "^[A-Za-z0-9_-]+$"))
            {
                errorProvider.SetError(txtMaLoai, "Mã chỉ gồm chữ không dấu, số, '-' hoặc '_'");
                hopLe = false;
            }

            if (txtTenLoai.Text.Trim().Length == 0)
            {
                errorProvider.SetError(txtTenLoai, "Vui lòng nhập tên loại");
                hopLe = false;
            }

            return hopLe;
        }

        private object MoTaHoacNull()
        {
            string moTa = txtMoTa.Text.Trim();
            return moTa.Length == 0 ? null : moTa;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (txtMaLoai.ReadOnly)
            {
                UiHelper.HienThongBao("Bấm 'Làm mới' trước khi nhập loại hàng mới.");
                return;
            }
            if (!KiemTra())
            {
                return;
            }

            string ma = txtMaLoai.Text.Trim();
            try
            {
                int daCo = Convert.ToInt32(Database.ExecuteScalar(
                    "SELECT COUNT(*) FROM LoaiHang WHERE MaLoai = @ma",
                    Database.Param("@ma", ma)));
                if (daCo > 0)
                {
                    errorProvider.SetError(txtMaLoai, "Mã loại đã tồn tại");
                    return;
                }

                Database.ExecuteNonQuery(
                    "INSERT INTO LoaiHang(MaLoai, TenLoai, MoTa) VALUES (@ma, @ten, @moTa)",
                    Database.Param("@ma", ma),
                    Database.Param("@ten", txtTenLoai.Text.Trim()),
                    Database.Param("@moTa", MoTaHoacNull()));

                TaiDuLieu();
                ChonDong(ma);
                UiHelper.HienThongBao("Thêm loại hàng thành công.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (!txtMaLoai.ReadOnly)
            {
                UiHelper.HienThongBao("Chọn loại hàng cần sửa trong danh sách.");
                return;
            }
            if (!KiemTra())
            {
                return;
            }

            string ma = txtMaLoai.Text.Trim();
            try
            {
                int soDong = Database.ExecuteNonQuery(
                    "UPDATE LoaiHang SET TenLoai = @ten, MoTa = @moTa WHERE MaLoai = @ma",
                    Database.Param("@ten", txtTenLoai.Text.Trim()),
                    Database.Param("@moTa", MoTaHoacNull()),
                    Database.Param("@ma", ma));

                if (soDong == 0)
                {
                    UiHelper.HienLoi("Không tìm thấy loại hàng cần sửa.");
                    return;
                }

                TaiDuLieu();
                ChonDong(ma);
                UiHelper.HienThongBao("Cập nhật loại hàng thành công.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (!txtMaLoai.ReadOnly)
            {
                UiHelper.HienThongBao("Chọn loại hàng cần xóa trong danh sách.");
                return;
            }

            string ma = txtMaLoai.Text.Trim();
            try
            {
                int soHang = Convert.ToInt32(Database.ExecuteScalar(
                    "SELECT COUNT(*) FROM HangHoa WHERE MaLoai = @ma",
                    Database.Param("@ma", ma)));
                if (soHang > 0)
                {
                    UiHelper.HienLoi("Không thể xóa: loại hàng này đang có " + soHang + " hàng hóa.");
                    return;
                }

                if (!UiHelper.XacNhan("Xóa loại hàng '" + txtTenLoai.Text.Trim() + "'?"))
                {
                    return;
                }

                Database.ExecuteNonQuery("DELETE FROM LoaiHang WHERE MaLoai = @ma", Database.Param("@ma", ma));
                TaiDuLieu();
                DatLaiForm();
                UiHelper.HienThongBao("Đã xóa loại hàng.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }
    }
}
