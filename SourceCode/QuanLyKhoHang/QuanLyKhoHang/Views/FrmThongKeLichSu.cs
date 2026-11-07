using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using QuanLyKhoHang.Presenters;
using QuanLyKhoHang.Services;

namespace QuanLyKhoHang.Views
{
    /// <summary>
    /// Giao diện Thống kê, Tra cứu lịch sử & Biểu đồ
    /// Triển khai theo mô hình MVP (implements IThongKeLichSuView)
    /// </summary>
    public class FrmThongKeLichSu : Form, IThongKeLichSuView
    {
        private readonly ThongKeLichSuPresenter _presenter;

        // Controls chính
        private TabControl tabControlChinh;

        // --- TAB 1: TRA CỨU LỊCH SỬ & IN PHIẾU ---
        private TabPage tabLichSu;
        private DateTimePicker dtpTuNgay;
        private DateTimePicker dtpDenNgay;
        private ComboBox cboLoaiPhieu;
        private TextBox txtTuKhoaLichSu;
        private Button btnTraCuuLichSu;
        private Button btnLamMoiLichSu;
        private Button btnXuatCsvLichSu;
        private Button btnInPhieu;
        private Button btnXemTruocIn;
        private DataGridView dgvPhieuKho;
        private DataGridView dgvChiTietPhieu;
        private Label lblTongSoPhieu;
        private Label lblTongTienNhap;
        private Label lblTongTienXuat;

        // --- TAB 2: TÍNH TỒN KHO & CẢNH BÁO ---
        private TabPage tabTonKho;
        private ComboBox cboLoaiHang;
        private ComboBox cboTrangThaiTon;
        private TextBox txtTuKhoaTon;
        private Button btnTinhTonKho;
        private Button btnLamMoiTon;
        private Button btnXuatCsvTonKho;
        private DataGridView dgvTonKho;
        private Label lblTongMatHang;
        private Label lblTongGiaTriTon;
        private Label lblSoHangCanhBao;

        // --- TAB 3: BIỂU ĐỒ GDI+ ---
        private TabPage tabBieuDo;
        private DateTimePicker dtpBieuDoTu;
        private DateTimePicker dtpBieuDoDen;
        private RadioButton radCot;
        private RadioButton radDuong;
        private Button btnVeBieuDo;
        private Button btnLuuAnhBieuDo;
        private Button btnXuatCsvBieuDo;
        private Panel pnlCanvas;
        private List<BieuDoGdiHelper.ItemThongKe> duLieuBieuDo = new List<BieuDoGdiHelper.ItemThongKe>();

        public FrmThongKeLichSu()
        {
            KhoiTaoGiaoDien();
            _presenter = new ThongKeLichSuPresenter(this);
        }

        #region IMPLEMENT IThongKeLichSuView PROPERTIES
        public DateTime TuNgay
        {
            get { return dtpTuNgay.Value; }
            set { dtpTuNgay.Value = value; }
        }

        public DateTime DenNgay
        {
            get { return dtpDenNgay.Value; }
            set { dtpDenNgay.Value = value; }
        }

        public string LoaiPhieu
        {
            get { return cboLoaiPhieu.SelectedItem != null ? cboLoaiPhieu.SelectedItem.ToString() : "Tất cả"; }
        }

        public string TuKhoaLichSu
        {
            get { return txtTuKhoaLichSu.Text.Trim(); }
        }

        public string MaLoaiHang
        {
            get { return cboLoaiHang.SelectedValue != null ? cboLoaiHang.SelectedValue.ToString() : ""; }
        }

        public string TrangThaiTon
        {
            get { return cboTrangThaiTon.SelectedItem != null ? cboTrangThaiTon.SelectedItem.ToString() : "Tất cả"; }
        }

        public string TuKhoaTon
        {
            get { return txtTuKhoaTon.Text.Trim(); }
        }

        public DateTime BieuDoTuNgay
        {
            get { return dtpBieuDoTu.Value; }
            set { dtpBieuDoTu.Value = value; }
        }

        public DateTime BieuDoDenNgay
        {
            get { return dtpBieuDoDen.Value; }
            set { dtpBieuDoDen.Value = value; }
        }

        public bool LaBieuDoCot
        {
            get { return radCot.Checked; }
        }
        #endregion

        #region IMPLEMENT IThongKeLichSuView METHODS
        public void HienThiDanhMucLoaiHang(DataTable dt)
        {
            cboLoaiHang.DisplayMember = "TenLoai";
            cboLoaiHang.ValueMember = "MaLoai";
            cboLoaiHang.DataSource = dt;
        }

        public void HienThiDanhSachPhieu(DataTable dt)
        {
            dgvPhieuKho.DataSource = dt;

            if (dgvPhieuKho.Columns["MaPhieu"] != null) dgvPhieuKho.Columns["MaPhieu"].HeaderText = "Mã phiếu";
            if (dgvPhieuKho.Columns["LoaiPhieu"] != null) dgvPhieuKho.Columns["LoaiPhieu"].HeaderText = "Loại phiếu";
            if (dgvPhieuKho.Columns["NgayLap"] != null)
            {
                dgvPhieuKho.Columns["NgayLap"].HeaderText = "Ngày lập";
                dgvPhieuKho.Columns["NgayLap"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }
            if (dgvPhieuKho.Columns["NguoiLap"] != null) dgvPhieuKho.Columns["NguoiLap"].HeaderText = "Người lập";
            if (dgvPhieuKho.Columns["DoiTac"] != null) dgvPhieuKho.Columns["DoiTac"].HeaderText = "Đối tác / Nhà cung cấp";
            if (dgvPhieuKho.Columns["SoMatHang"] != null)
            {
                dgvPhieuKho.Columns["SoMatHang"].HeaderText = "Số mặt hàng";
                dgvPhieuKho.Columns["SoMatHang"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            if (dgvPhieuKho.Columns["TongSoLuong"] != null)
            {
                dgvPhieuKho.Columns["TongSoLuong"].HeaderText = "Tổng SL";
                dgvPhieuKho.Columns["TongSoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvPhieuKho.Columns["TongSoLuong"].DefaultCellStyle.Format = "N0";
            }
            if (dgvPhieuKho.Columns["TongTien"] != null)
            {
                dgvPhieuKho.Columns["TongTien"].HeaderText = "Tổng tiền (VNĐ)";
                dgvPhieuKho.Columns["TongTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgvPhieuKho.Columns["TongTien"].DefaultCellStyle.Format = "#,##0";
            }
            if (dgvPhieuKho.Columns["GhiChu"] != null) dgvPhieuKho.Columns["GhiChu"].HeaderText = "Ghi chú";

            if (dt == null || dt.Rows.Count == 0)
            {
                dgvChiTietPhieu.DataSource = null;
            }
        }

        public void HienThiChiTietPhieu(DataTable dt)
        {
            dgvChiTietPhieu.DataSource = dt;

            if (dt != null)
            {
                if (dgvChiTietPhieu.Columns["MaHang"] != null) dgvChiTietPhieu.Columns["MaHang"].HeaderText = "Mã hàng";
                if (dgvChiTietPhieu.Columns["TenHang"] != null) dgvChiTietPhieu.Columns["TenHang"].HeaderText = "Tên mặt hàng";
                if (dgvChiTietPhieu.Columns["TenLoai"] != null) dgvChiTietPhieu.Columns["TenLoai"].HeaderText = "Loại hàng";
                if (dgvChiTietPhieu.Columns["DonViTinh"] != null) dgvChiTietPhieu.Columns["DonViTinh"].HeaderText = "ĐVT";
                if (dgvChiTietPhieu.Columns["SoLuong"] != null)
                {
                    dgvChiTietPhieu.Columns["SoLuong"].HeaderText = "Số lượng";
                    dgvChiTietPhieu.Columns["SoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvChiTietPhieu.Columns["SoLuong"].DefaultCellStyle.Format = "N0";
                }
                if (dgvChiTietPhieu.Columns["DonGia"] != null)
                {
                    dgvChiTietPhieu.Columns["DonGia"].HeaderText = "Đơn giá (VNĐ)";
                    dgvChiTietPhieu.Columns["DonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvChiTietPhieu.Columns["DonGia"].DefaultCellStyle.Format = "#,##0";
                }
                if (dgvChiTietPhieu.Columns["ThanhTien"] != null)
                {
                    dgvChiTietPhieu.Columns["ThanhTien"].HeaderText = "Thành tiền (VNĐ)";
                    dgvChiTietPhieu.Columns["ThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvChiTietPhieu.Columns["ThanhTien"].DefaultCellStyle.Format = "#,##0";
                }
            }
        }

        public void HienThiBaoCaoTonKho(DataTable dt)
        {
            dgvTonKho.DataSource = dt;

            if (dt != null)
            {
                if (dgvTonKho.Columns["MaHang"] != null) dgvTonKho.Columns["MaHang"].HeaderText = "Mã hàng";
                if (dgvTonKho.Columns["TenHang"] != null) dgvTonKho.Columns["TenHang"].HeaderText = "Tên mặt hàng";
                if (dgvTonKho.Columns["TenLoai"] != null) dgvTonKho.Columns["TenLoai"].HeaderText = "Loại hàng";
                if (dgvTonKho.Columns["DonViTinh"] != null) dgvTonKho.Columns["DonViTinh"].HeaderText = "ĐVT";
                if (dgvTonKho.Columns["DonGiaNhap"] != null)
                {
                    dgvTonKho.Columns["DonGiaNhap"].HeaderText = "Giá nhập (VNĐ)";
                    dgvTonKho.Columns["DonGiaNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["DonGiaNhap"].DefaultCellStyle.Format = "#,##0";
                }
                if (dgvTonKho.Columns["TongNhap"] != null)
                {
                    dgvTonKho.Columns["TongNhap"].HeaderText = "Tổng nhập";
                    dgvTonKho.Columns["TongNhap"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["TongNhap"].DefaultCellStyle.Format = "N0";
                }
                if (dgvTonKho.Columns["TongXuat"] != null)
                {
                    dgvTonKho.Columns["TongXuat"].HeaderText = "Tổng xuất";
                    dgvTonKho.Columns["TongXuat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["TongXuat"].DefaultCellStyle.Format = "N0";
                }
                if (dgvTonKho.Columns["SoLuongTon"] != null)
                {
                    dgvTonKho.Columns["SoLuongTon"].HeaderText = "Tồn kho";
                    dgvTonKho.Columns["SoLuongTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["SoLuongTon"].DefaultCellStyle.Format = "N0";
                }
                if (dgvTonKho.Columns["MucTonToiThieu"] != null)
                {
                    dgvTonKho.Columns["MucTonToiThieu"].HeaderText = "Tối thiểu";
                    dgvTonKho.Columns["MucTonToiThieu"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["MucTonToiThieu"].DefaultCellStyle.Format = "N0";
                }
                if (dgvTonKho.Columns["GiaTriTon"] != null)
                {
                    dgvTonKho.Columns["GiaTriTon"].HeaderText = "Giá trị tồn (VNĐ)";
                    dgvTonKho.Columns["GiaTriTon"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dgvTonKho.Columns["GiaTriTon"].DefaultCellStyle.Format = "#,##0";
                }
                if (dgvTonKho.Columns["TrangThai"] != null)
                {
                    dgvTonKho.Columns["TrangThai"].HeaderText = "Trạng thái";
                    dgvTonKho.Columns["TrangThai"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                if (dgvTonKho.Columns["MaLoai"] != null) dgvTonKho.Columns["MaLoai"].Visible = false;
            }
        }

        public void CapNhatBieuDo(List<BieuDoGdiHelper.ItemThongKe> duLieu)
        {
            duLieuBieuDo = duLieu ?? new List<BieuDoGdiHelper.ItemThongKe>();
            pnlCanvas.Invalidate();
        }

        public void CapNhatThongKeLichSu(int soPhieu, decimal tongNhap, decimal tongXuat)
        {
            lblTongSoPhieu.Text = "Tổng: " + soPhieu + " phiếu";
            lblTongTienNhap.Text = "Tổng tiền nhập: " + tongNhap.ToString("#,##0") + " VNĐ";
            lblTongTienXuat.Text = "Tổng tiền xuất: " + tongXuat.ToString("#,##0") + " VNĐ";
        }

        public void CapNhatThongKeTonKho(int soMatHang, decimal tongGiaTriTon, int soCanhBao)
        {
            lblTongMatHang.Text = "Số mặt hàng: " + soMatHang;
            lblTongGiaTriTon.Text = "Tổng giá trị tồn: " + tongGiaTriTon.ToString("#,##0") + " VNĐ";
            lblSoHangCanhBao.Text = "Hàng cảnh báo / hết: " + soCanhBao;
        }

        public void HienThongBao(string thongBao)
        {
            MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void HienThongBaoLoi(string loi)
        {
            MessageBox.Show(loi, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public IWin32Window GetWindow()
        {
            return this;
        }
        #endregion

        #region KHỞI TẠO GIAO DIỆN FORM
        private void KhoiTaoGiaoDien()
        {
            Text = "Quản lý kho hàng - Tra cứu lịch sử, Tính tồn kho & Báo cáo biểu đồ (MVP)";
            Size = new Size(1150, 720);
            MinimumSize = new Size(950, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.25f);
            BackColor = Color.FromArgb(248, 250, 252);

            tabControlChinh = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(14, 8)
            };

            KhoiTaoTabLichSu();
            KhoiTaoTabTonKho();
            KhoiTaoTabBieuDo();

            tabControlChinh.TabPages.Add(tabLichSu);
            tabControlChinh.TabPages.Add(tabTonKho);
            tabControlChinh.TabPages.Add(tabBieuDo);

            Controls.Add(tabControlChinh);

            Load += (s, e) =>
            {
                _presenter.NapDanhMucLoaiHang();
                _presenter.TaiLichSuNhapXuat();
                _presenter.TaiBaoCaoTonKho();
                _presenter.TaiDuLieuBieuDo();
            };
        }

        private void KhoiTaoTabLichSu()
        {
            tabLichSu = new TabPage("1. Tra cứu Lịch sử & In Phiếu Kho");

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            Label lblTu = new Label { Text = "Từ ngày:", Location = new Point(12, 14), AutoSize = true };
            dtpTuNgay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(75, 11),
                Width = 110,
                Value = DateTime.Today.AddMonths(-1)
            };

            Label lblDen = new Label { Text = "Đến ngày:", Location = new Point(195, 14), AutoSize = true };
            dtpDenNgay = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(265, 11),
                Width = 110,
                Value = DateTime.Today
            };

            Label lblLoai = new Label { Text = "Loại phiếu:", Location = new Point(385, 14), AutoSize = true };
            cboLoaiPhieu = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(455, 11),
                Width = 100
            };
            cboLoaiPhieu.Items.AddRange(new object[] { "Tất cả", "Nhập", "Xuất" });
            cboLoaiPhieu.SelectedIndex = 0;

            Label lblTim = new Label { Text = "Từ khóa:", Location = new Point(565, 14), AutoSize = true };
            txtTuKhoaLichSu = new TextBox { Location = new Point(625, 11), Width = 140 };

            btnTraCuuLichSu = TaoButton("Tra cứu", Color.FromArgb(37, 99, 235), Color.White, 775, 9, 85);
            btnLamMoiLichSu = TaoButton("Làm mới", Color.FromArgb(100, 116, 139), Color.White, 868, 9, 80);

            btnInPhieu = TaoButton("In phiếu (PrintDocument)", Color.FromArgb(16, 185, 129), Color.White, 12, 48, 185);
            btnXemTruocIn = TaoButton("Xem trước bản in", Color.FromArgb(13, 148, 136), Color.White, 205, 48, 140);
            btnXuatCsvLichSu = TaoButton("Xuất file CSV", Color.FromArgb(217, 119, 6), Color.White, 353, 48, 120);

            lblTongSoPhieu = new Label { Text = "Tổng: 0 phiếu", Location = new Point(490, 53), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTongTienNhap = new Label { Text = "Tiền nhập: 0 đ", Location = new Point(610, 53), AutoSize = true, ForeColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTongTienXuat = new Label { Text = "Tiền xuất: 0 đ", Location = new Point(810, 53), AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            pnlTop.Controls.AddRange(new Control[] {
                lblTu, dtpTuNgay, lblDen, dtpDenNgay, lblLoai, cboLoaiPhieu, lblTim, txtTuKhoaLichSu,
                btnTraCuuLichSu, btnLamMoiLichSu, btnInPhieu, btnXemTruocIn, btnXuatCsvLichSu,
                lblTongSoPhieu, lblTongTienNhap, lblTongTienXuat
            });

            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 340,
                Padding = new Padding(6)
            };

            GroupBox grpMaster = new GroupBox { Text = "Danh sách phiếu nhập / xuất kho", Dock = DockStyle.Fill, Padding = new Padding(8) };
            dgvPhieuKho = TaoDataGridView();
            grpMaster.Controls.Add(dgvPhieuKho);
            splitContainer.Panel1.Controls.Add(grpMaster);

            GroupBox grpDetail = new GroupBox { Text = "Chi tiết mặt hàng của phiếu đang chọn", Dock = DockStyle.Fill, Padding = new Padding(8) };
            dgvChiTietPhieu = TaoDataGridView();
            grpDetail.Controls.Add(dgvChiTietPhieu);
            splitContainer.Panel2.Controls.Add(grpDetail);

            tabLichSu.Controls.Add(splitContainer);
            tabLichSu.Controls.Add(pnlTop);

            // Gán sự kiện gọi Presenter
            btnTraCuuLichSu.Click += (s, e) => _presenter.TaiLichSuNhapXuat();
            btnLamMoiLichSu.Click += (s, e) =>
            {
                dtpTuNgay.Value = DateTime.Today.AddMonths(-1);
                dtpDenNgay.Value = DateTime.Today;
                cboLoaiPhieu.SelectedIndex = 0;
                txtTuKhoaLichSu.Clear();
                _presenter.TaiLichSuNhapXuat();
            };
            btnXuatCsvLichSu.Click += (s, e) => _presenter.XuatCsvLichSu(dgvPhieuKho);
            btnInPhieu.Click += (s, e) =>
            {
                string ma = LayMaPhieuDangChon();
                _presenter.InPhieuKho(ma, false);
            };
            btnXemTruocIn.Click += (s, e) =>
            {
                string ma = LayMaPhieuDangChon();
                _presenter.InPhieuKho(ma, true);
            };

            dgvPhieuKho.SelectionChanged += (s, e) =>
            {
                string ma = LayMaPhieuDangChon();
                _presenter.TaiChiTietPhieu(ma);
            };
        }

        private string LayMaPhieuDangChon()
        {
            if (dgvPhieuKho.CurrentRow != null && dgvPhieuKho.CurrentRow.Index >= 0)
            {
                object val = dgvPhieuKho.CurrentRow.Cells["MaPhieu"].Value;
                return val != null ? val.ToString() : null;
            }
            return null;
        }

        private void KhoiTaoTabTonKho()
        {
            tabTonKho = new TabPage("2. Tính Tồn Kho (Stored Procedure) & Cảnh báo");

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            Label lblLoai = new Label { Text = "Loại hàng:", Location = new Point(12, 14), AutoSize = true };
            cboLoaiHang = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(80, 11),
                Width = 150
            };

            Label lblTrangThai = new Label { Text = "Trạng thái:", Location = new Point(245, 14), AutoSize = true };
            cboTrangThaiTon = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(315, 11),
                Width = 135
            };
            cboTrangThaiTon.Items.AddRange(new object[] { "Tất cả", "Cảnh báo tồn thấp", "Hết hàng", "An toàn" });
            cboTrangThaiTon.SelectedIndex = 0;

            Label lblTim = new Label { Text = "Tìm kiếm:", Location = new Point(465, 14), AutoSize = true };
            txtTuKhoaTon = new TextBox { Location = new Point(530, 11), Width = 150 };

            btnTinhTonKho = TaoButton("Tính tồn kho (Stored Proc)", Color.FromArgb(37, 99, 235), Color.White, 695, 9, 180);
            btnLamMoiTon = TaoButton("Làm mới", Color.FromArgb(100, 116, 139), Color.White, 885, 9, 80);

            btnXuatCsvTonKho = TaoButton("Xuất file CSV Báo cáo", Color.FromArgb(217, 119, 6), Color.White, 12, 48, 160);

            lblTongMatHang = new Label { Text = "Số mặt hàng: 0", Location = new Point(200, 53), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTongGiaTriTon = new Label { Text = "Tổng giá trị tồn: 0 đ", Location = new Point(360, 53), AutoSize = true, ForeColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblSoHangCanhBao = new Label { Text = "Cảnh báo tồn thấp: 0", Location = new Point(620, 53), AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            pnlTop.Controls.AddRange(new Control[] {
                lblLoai, cboLoaiHang, lblTrangThai, cboTrangThaiTon, lblTim, txtTuKhoaTon,
                btnTinhTonKho, btnLamMoiTon, btnXuatCsvTonKho,
                lblTongMatHang, lblTongGiaTriTon, lblSoHangCanhBao
            });

            GroupBox grpTonKho = new GroupBox
            {
                Text = "Bảng dữ liệu tồn kho thực tế tính qua Stored Procedure",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            dgvTonKho = TaoDataGridView();
            grpTonKho.Controls.Add(dgvTonKho);

            tabTonKho.Controls.Add(grpTonKho);
            tabTonKho.Controls.Add(pnlTop);

            btnTinhTonKho.Click += (s, e) => _presenter.TaiBaoCaoTonKho();
            btnLamMoiTon.Click += (s, e) =>
            {
                cboLoaiHang.SelectedIndex = 0;
                cboTrangThaiTon.SelectedIndex = 0;
                txtTuKhoaTon.Clear();
                _presenter.TaiBaoCaoTonKho();
            };
            btnXuatCsvTonKho.Click += (s, e) => _presenter.XuatCsvTonKho(dgvTonKho);

            dgvTonKho.CellFormatting += DgvTonKho_CellFormatting;
        }

        private void DgvTonKho_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvTonKho.Rows.Count) return;

            DataGridViewRow row = dgvTonKho.Rows[e.RowIndex];
            if (row.Cells["TrangThai"] != null && row.Cells["TrangThai"].Value != null)
            {
                string tt = row.Cells["TrangThai"].Value.ToString();
                if (tt == "Hết hàng")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                }
                else if (tt == "Cảnh báo tồn thấp")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                }
            }
        }

        private void KhoiTaoTabBieuDo()
        {
            tabBieuDo = new TabPage("3. Biểu đồ Nhập - Xuất - Tồn (GDI+)");

            Panel pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                Padding = new Padding(10),
                BackColor = Color.White
            };

            Label lblTu = new Label { Text = "Từ ngày:", Location = new Point(12, 20), AutoSize = true };
            dtpBieuDoTu = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(75, 17),
                Width = 110,
                Value = DateTime.Today.AddMonths(-2)
            };

            Label lblDen = new Label { Text = "Đến ngày:", Location = new Point(195, 20), AutoSize = true };
            dtpBieuDoDen = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy",
                Location = new Point(265, 17),
                Width = 110,
                Value = DateTime.Today
            };

            radCot = new RadioButton { Text = "Biểu đồ cột nhóm", Location = new Point(390, 18), AutoSize = true, Checked = true };
            radDuong = new RadioButton { Text = "Biểu đồ đường", Location = new Point(520, 18), AutoSize = true };

            btnVeBieuDo = TaoButton("Tải dữ liệu & Vẽ lại", Color.FromArgb(37, 99, 235), Color.White, 650, 15, 140);
            btnLuuAnhBieuDo = TaoButton("Lưu ảnh biểu đồ (PNG)", Color.FromArgb(16, 185, 129), Color.White, 800, 15, 160);
            btnXuatCsvBieuDo = TaoButton("Xuất CSV Số liệu", Color.FromArgb(217, 119, 6), Color.White, 970, 15, 130);

            pnlTop.Controls.AddRange(new Control[] {
                lblTu, dtpBieuDoTu, lblDen, dtpBieuDoDen, radCot, radDuong,
                btnVeBieuDo, btnLuuAnhBieuDo, btnXuatCsvBieuDo
            });

            pnlCanvas = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            pnlCanvas.Paint += PnlCanvas_Paint;

            tabBieuDo.Controls.Add(pnlCanvas);
            tabBieuDo.Controls.Add(pnlTop);

            btnVeBieuDo.Click += (s, e) => _presenter.TaiDuLieuBieuDo();
            radCot.CheckedChanged += (s, e) => { if (radCot.Checked) pnlCanvas.Invalidate(); };
            radDuong.CheckedChanged += (s, e) => { if (radDuong.Checked) pnlCanvas.Invalidate(); };
            btnLuuAnhBieuDo.Click += (s, e) => _presenter.LuuAnhBieuDo(duLieuBieuDo, radCot.Checked);
            btnXuatCsvBieuDo.Click += (s, e) => _presenter.XuatCsvBieuDo(duLieuBieuDo);
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            BieuDoGdiHelper.LoaiBieuDo loai = radCot.Checked ? BieuDoGdiHelper.LoaiBieuDo.CotNhom : BieuDoGdiHelper.LoaiBieuDo.Duong;
            string tieuDe = string.Format("BIỂU ĐỒ SO SÁNH NHẬP - XUẤT - TỒN (Từ {0:dd/MM/yyyy} đến {1:dd/MM/yyyy})", dtpBieuDoTu.Value, dtpBieuDoDen.Value);
            BieuDoGdiHelper.VeBieuDo(e.Graphics, pnlCanvas.ClientRectangle, duLieuBieuDo, loai, tieuDe);
        }
        #endregion

        #region UTILITIES GIAO DIỆN
        private Button TaoButton(string text, Color bg, Color fg, int x, int y, int width, int height = 30)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private DataGridView TaoDataGridView()
        {
            DataGridView dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.25f, FontStyle.Bold);
            dgv.ColumnHeadersHeight = 34;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 231, 255);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.RowTemplate.Height = 28;

            return dgv;
        }

        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                UpdateStyles();
            }
        }
        #endregion
    }
}
