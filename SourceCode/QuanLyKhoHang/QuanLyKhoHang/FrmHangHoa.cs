using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmHangHoa : Form
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

        public FrmHangHoa()
        {
            KhoiTaoGiaoDien();
            TaiDanhSachLoai();
            TaiDuLieu();
            PhanQuyen();
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

            btnThem.Click += BtnThem_Click;
            btnSua.Click += BtnSua_Click;
            btnXoa.Click += BtnXoa_Click;
            btnLamMoi.Click += (s, e) => DatLaiForm();
            btnChonAnh.Click += BtnChonAnh_Click;
            btnBoAnh.Click += (s, e) => HienAnh(null);
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
            if (choPhep)
            {
                TaiDuLieu();
            }
        }

        private void TaiDanhSachLoai()
        {
            bool truoc = choPhep;
            choPhep = false;
            try
            {
                DataTable dtLoai = Database.GetData("SELECT MaLoai, TenLoai FROM LoaiHang ORDER BY TenLoai");
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
            catch (Exception ex)
            {
                UiHelper.HienLoi("Không tải được danh sách loại hàng: " + ex.Message);
            }
            finally
            {
                choPhep = truoc;
            }
        }

        private void TaiDuLieu()
        {
            bool truoc = choPhep;
            choPhep = false;
            try
            {
                string maLoaiLoc = cboLoc.SelectedValue as string ?? string.Empty;
                DataTable dt = Database.GetDataProc(
                    "sp_LayDanhSachHangHoa",
                    Database.Param("@TuKhoa", txtTimKiem.Text.Trim()),
                    Database.Param("@MaLoai", maLoaiLoc));

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
            catch (Exception ex)
            {
                UiHelper.HienLoi("Không tải được dữ liệu: " + ex.Message);
            }
            finally
            {
                choPhep = truoc;
            }
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

        private void DatLaiForm()
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

        private void ChonDong(string ma)
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

        private bool KiemTra()
        {
            errorProvider.Clear();
            bool hopLe = true;

            string ma = txtMaHang.Text.Trim();
            if (ma.Length == 0)
            {
                errorProvider.SetError(txtMaHang, "Vui lòng nhập mã hàng");
                hopLe = false;
            }
            else if (!Regex.IsMatch(ma, "^[A-Za-z0-9_-]+$"))
            {
                errorProvider.SetError(txtMaHang, "Mã chỉ gồm chữ không dấu, số, '-' hoặc '_'");
                hopLe = false;
            }

            if (txtTenHang.Text.Trim().Length == 0)
            {
                errorProvider.SetError(txtTenHang, "Vui lòng nhập tên hàng");
                hopLe = false;
            }

            if (cboLoai.SelectedValue == null)
            {
                errorProvider.SetError(cboLoai, "Vui lòng chọn loại hàng");
                hopLe = false;
            }

            if (txtDonViTinh.Text.Trim().Length == 0)
            {
                errorProvider.SetError(txtDonViTinh, "Vui lòng nhập đơn vị tính");
                hopLe = false;
            }

            if (nudDonGiaNhap.Value <= 0)
            {
                errorProvider.SetError(nudDonGiaNhap, "Đơn giá nhập phải lớn hơn 0");
                hopLe = false;
            }

            return hopLe;
        }

        private void BtnThem_Click(object sender, EventArgs e)
        {
            if (txtMaHang.ReadOnly)
            {
                UiHelper.HienThongBao("Bấm 'Làm mới' trước khi nhập hàng hóa mới.");
                return;
            }
            if (!KiemTra())
            {
                return;
            }

            string ma = txtMaHang.Text.Trim();
            try
            {
                int daCo = Convert.ToInt32(Database.ExecuteScalar(
                    "SELECT COUNT(*) FROM HangHoa WHERE MaHang = @ma",
                    Database.Param("@ma", ma)));
                if (daCo > 0)
                {
                    errorProvider.SetError(txtMaHang, "Mã hàng đã tồn tại");
                    return;
                }

                Database.ExecuteNonQuery(
                    "INSERT INTO HangHoa(MaHang, TenHang, MaLoai, DonViTinh, DonGiaNhap, SoLuongTon, MucTonToiThieu, HinhAnh) " +
                    "VALUES (@ma, @ten, @loai, @dvt, @gia, @ton, @toiThieu, @anh)",
                    Database.Param("@ma", ma),
                    Database.Param("@ten", txtTenHang.Text.Trim()),
                    Database.Param("@loai", cboLoai.SelectedValue),
                    Database.Param("@dvt", txtDonViTinh.Text.Trim()),
                    Database.Param("@gia", nudDonGiaNhap.Value),
                    Database.Param("@ton", (int)nudTon.Value),
                    Database.Param("@toiThieu", (int)nudTonToiThieu.Value),
                    Database.Param("@anh", duongDanAnh));

                TaiDuLieu();
                ChonDong(ma);
                UiHelper.HienThongBao("Thêm hàng hóa thành công.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void BtnSua_Click(object sender, EventArgs e)
        {
            if (!txtMaHang.ReadOnly)
            {
                UiHelper.HienThongBao("Chọn hàng hóa cần sửa trong danh sách.");
                return;
            }
            if (!KiemTra())
            {
                return;
            }

            string ma = txtMaHang.Text.Trim();
            try
            {
                int soDong = Database.ExecuteNonQuery(
                    "UPDATE HangHoa SET TenHang = @ten, MaLoai = @loai, DonViTinh = @dvt, DonGiaNhap = @gia, " +
                    "MucTonToiThieu = @toiThieu, HinhAnh = @anh WHERE MaHang = @ma",
                    Database.Param("@ten", txtTenHang.Text.Trim()),
                    Database.Param("@loai", cboLoai.SelectedValue),
                    Database.Param("@dvt", txtDonViTinh.Text.Trim()),
                    Database.Param("@gia", nudDonGiaNhap.Value),
                    Database.Param("@toiThieu", (int)nudTonToiThieu.Value),
                    Database.Param("@anh", duongDanAnh),
                    Database.Param("@ma", ma));

                if (soDong == 0)
                {
                    UiHelper.HienLoi("Không tìm thấy hàng hóa cần sửa.");
                    return;
                }

                TaiDuLieu();
                ChonDong(ma);
                UiHelper.HienThongBao("Cập nhật hàng hóa thành công.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (!txtMaHang.ReadOnly)
            {
                UiHelper.HienThongBao("Chọn hàng hóa cần xóa trong danh sách.");
                return;
            }

            string ma = txtMaHang.Text.Trim();
            try
            {
                int soPhieu = Convert.ToInt32(Database.ExecuteScalar(
                    "SELECT COUNT(*) FROM ChiTietPhieu WHERE MaHang = @ma",
                    Database.Param("@ma", ma)));
                if (soPhieu > 0)
                {
                    UiHelper.HienLoi("Không thể xóa: hàng hóa này đã phát sinh " + soPhieu + " dòng nhập/xuất kho.");
                    return;
                }

                if (!UiHelper.XacNhan("Xóa hàng hóa '" + txtTenHang.Text.Trim() + "'?"))
                {
                    return;
                }

                Database.ExecuteNonQuery("DELETE FROM HangHoa WHERE MaHang = @ma", Database.Param("@ma", ma));
                TaiDuLieu();
                DatLaiForm();
                UiHelper.HienThongBao("Đã xóa hàng hóa.");
            }
            catch (SqlException ex)
            {
                UiHelper.HienLoi(UiHelper.MoTaLoiSql(ex));
            }
        }
    }
}
