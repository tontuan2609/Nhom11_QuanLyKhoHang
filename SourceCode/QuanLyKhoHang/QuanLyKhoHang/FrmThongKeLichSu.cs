using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmThongKeLichSu : Form
    {
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
        }

        private void KhoiTaoGiaoDien()
        {
            Text = "Quản lý kho hàng - Tra cứu lịch sử, Tính tồn kho & Báo cáo biểu đồ";
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

            // Nạp dữ liệu ban đầu khi mở form
            Load += (s, e) =>
            {
                NapDanhMucLoaiHang();
                TaiLichSuNhapXuat();
                TaiBaoCaoTonKho();
                TaiDuLieuBieuDo();
            };
        }

        #region 1. TAB TRA CỨU LỊCH SỬ & IN PHIẾU
        private void KhoiTaoTabLichSu()
        {
            tabLichSu = new TabPage("1. Tra cứu Lịch sử & In Phiếu Kho");

            // Panel thanh công cụ lọc trên cùng
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
                Value = DateTime.Today.AddMonths(-1) // 1 tháng trước
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
            txtTuKhoaLichSu = new TextBox
            {
                Location = new Point(625, 11),
                Width = 140
            };

            btnTraCuuLichSu = TaoButton("Tra cứu", Color.FromArgb(37, 99, 235), Color.White, 775, 9, 85);
            btnLamMoiLichSu = TaoButton("Làm mới", Color.FromArgb(100, 116, 139), Color.White, 868, 9, 80);

            // Hàng nút chức năng bên dưới
            btnInPhieu = TaoButton("In phiếu (PrintDocument)", Color.FromArgb(16, 185, 129), Color.White, 12, 48, 185);
            btnXemTruocIn = TaoButton("Xem trước bản in", Color.FromArgb(13, 148, 136), Color.White, 205, 48, 140);
            btnXuatCsvLichSu = TaoButton("Xuất file CSV", Color.FromArgb(217, 119, 6), Color.White, 353, 48, 120);

            // Thanh tóm tắt tổng số tiền
            lblTongSoPhieu = new Label { Text = "Tổng: 0 phiếu", Location = new Point(490, 53), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTongTienNhap = new Label { Text = "Tiền nhập: 0 đ", Location = new Point(610, 53), AutoSize = true, ForeColor = Color.FromArgb(37, 99, 235), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            lblTongTienXuat = new Label { Text = "Tiền xuất: 0 đ", Location = new Point(810, 53), AutoSize = true, ForeColor = Color.FromArgb(220, 38, 38), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };

            pnlTop.Controls.AddRange(new Control[] {
                lblTu, dtpTuNgay, lblDen, dtpDenNgay, lblLoai, cboLoaiPhieu, lblTim, txtTuKhoaLichSu,
                btnTraCuuLichSu, btnLamMoiLichSu, btnInPhieu, btnXemTruocIn, btnXuatCsvLichSu,
                lblTongSoPhieu, lblTongTienNhap, lblTongTienXuat
            });

            // SplitContainer chia trên (Danh sách phiếu) và dưới (Chi tiết phiếu được chọn)
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 340,
                Padding = new Padding(6)
            };

            // Master: Danh sách phiếu
            GroupBox grpMaster = new GroupBox { Text = "Danh sách phiếu nhập / xuất kho", Dock = DockStyle.Fill, Padding = new Padding(8) };
            dgvPhieuKho = TaoDataGridView();
            grpMaster.Controls.Add(dgvPhieuKho);
            splitContainer.Panel1.Controls.Add(grpMaster);

            // Detail: Chi tiết phiếu
            GroupBox grpDetail = new GroupBox { Text = "Chi tiết mặt hàng của phiếu đang chọn", Dock = DockStyle.Fill, Padding = new Padding(8) };
            dgvChiTietPhieu = TaoDataGridView();
            grpDetail.Controls.Add(dgvChiTietPhieu);
            splitContainer.Panel2.Controls.Add(grpDetail);

            tabLichSu.Controls.Add(splitContainer);
            tabLichSu.Controls.Add(pnlTop);

            // Gán sự kiện
            btnTraCuuLichSu.Click += (s, e) => TaiLichSuNhapXuat();
            btnLamMoiLichSu.Click += (s, e) =>
            {
                dtpTuNgay.Value = DateTime.Today.AddMonths(-1);
                dtpDenNgay.Value = DateTime.Today;
                cboLoaiPhieu.SelectedIndex = 0;
                txtTuKhoaLichSu.Clear();
                TaiLichSuNhapXuat();
            };
            btnXuatCsvLichSu.Click += (s, e) => CsvHelperCustom.XuatDataGridViewRaCsv(dgvPhieuKho, "LichSuNhapXuat", this);
            btnInPhieu.Click += (s, e) => ThucHienInPhieu(false);
            btnXemTruocIn.Click += (s, e) => ThucHienInPhieu(true);

            dgvPhieuKho.SelectionChanged += (s, e) =>
            {
                if (dgvPhieuKho.CurrentRow != null && dgvPhieuKho.CurrentRow.Index >= 0)
                {
                    object val = dgvPhieuKho.CurrentRow.Cells["MaPhieu"].Value;
                    string maPhieu = val != null ? val.ToString() : null;
                    TaiChiTietPhieu(maPhieu);
                }
            };
        }

        private void TaiLichSuNhapXuat()
        {
            try
            {
                string sql = @"
                    SELECT 
                        p.MaPhieu,
                        p.LoaiPhieu,
                        p.NgayLap,
                        nd.HoTen AS NguoiLap,
                        ISNULL(ncc.TenNCC, N'(Khách lẻ / Đơn vị ngoài)') AS DoiTac,
                        COUNT(ct.MaHang) AS SoMatHang,
                        ISNULL(SUM(ct.SoLuong), 0) AS TongSoLuong,
                        p.TongTien,
                        p.GhiChu
                    FROM PhieuKho p
                    INNER JOIN NguoiDung nd ON p.MaNguoiDung = nd.MaNguoiDung
                    LEFT JOIN NhaCungCap ncc ON p.MaNCC = ncc.MaNCC
                    LEFT JOIN ChiTietPhieu ct ON p.MaPhieu = ct.MaPhieu
                    WHERE p.NgayLap >= @TuNgay AND p.NgayLap < @DenNgayChuan
                      AND (@LoaiPhieu = N'Tất cả' OR p.LoaiPhieu = @LoaiPhieu)
                      AND (
                          @TuKhoa = N'' 
                          OR p.MaPhieu LIKE '%' + @TuKhoa + '%'
                          OR nd.HoTen LIKE N'%' + @TuKhoa + '%'
                          OR ISNULL(ncc.TenNCC, '') LIKE N'%' + @TuKhoa + '%'
                          OR ISNULL(p.GhiChu, '') LIKE N'%' + @TuKhoa + '%'
                      )
                    GROUP BY p.MaPhieu, p.LoaiPhieu, p.NgayLap, nd.HoTen, ncc.TenNCC, p.TongTien, p.GhiChu
                    ORDER BY p.NgayLap DESC";

                DateTime denNgayChuan = dtpDenNgay.Value.Date.AddDays(1);

                DataTable dt = Database.GetData(sql,
                    Database.Param("@TuNgay", dtpTuNgay.Value.Date),
                    Database.Param("@DenNgayChuan", denNgayChuan),
                    Database.Param("@LoaiPhieu", cboLoaiPhieu.SelectedItem.ToString()),
                    Database.Param("@TuKhoa", txtTuKhoaLichSu.Text.Trim()));

                dgvPhieuKho.DataSource = dt;

                // Định dạng hiển thị cột
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

                // Tính tổng cộng
                decimal tongNhap = 0;
                decimal tongXuat = 0;
                foreach (DataRow r in dt.Rows)
                {
                    string lp = r["LoaiPhieu"].ToString();
                    decimal tien = Convert.ToDecimal(r["TongTien"]);
                    if (lp == "Nhập") tongNhap += tien;
                    else if (lp == "Xuất") tongXuat += tien;
                }

                lblTongSoPhieu.Text = "Tổng: " + dt.Rows.Count + " phiếu";
                lblTongTienNhap.Text = "Tổng tiền nhập: " + tongNhap.ToString("#,##0") + " VNĐ";
                lblTongTienXuat.Text = "Tổng tiền xuất: " + tongXuat.ToString("#,##0") + " VNĐ";

                if (dt.Rows.Count == 0)
                {
                    dgvChiTietPhieu.DataSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải lịch sử: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiChiTietPhieu(string maPhieu)
        {
            if (string.IsNullOrEmpty(maPhieu))
            {
                dgvChiTietPhieu.DataSource = null;
                return;
            }

            try
            {
                string sql = @"
                    SELECT 
                        ct.MaHang,
                        h.TenHang,
                        l.TenLoai,
                        h.DonViTinh,
                        ct.SoLuong,
                        ct.DonGia,
                        ct.ThanhTien
                    FROM ChiTietPhieu ct
                    INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                    INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
                    WHERE ct.MaPhieu = @MaPhieu
                    ORDER BY ct.MaHang ASC";

                DataTable dt = Database.GetData(sql, Database.Param("@MaPhieu", maPhieu));
                dgvChiTietPhieu.DataSource = dt;

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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải chi tiết phiếu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ThucHienInPhieu(bool xemTruoc)
        {
            if (dgvPhieuKho.CurrentRow == null || dgvPhieuKho.CurrentRow.Index < 0)
            {
                MessageBox.Show("Vui lòng chọn một phiếu kho trên danh sách để in!", "Chưa chọn phiếu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object val = dgvPhieuKho.CurrentRow.Cells["MaPhieu"].Value;
            string maPhieu = val != null ? val.ToString() : null;
            if (string.IsNullOrEmpty(maPhieu)) return;

            PhieuKhoPrintHelper printer = new PhieuKhoPrintHelper(maPhieu);
            if (xemTruoc)
            {
                printer.XemTruoc(this);
            }
            else
            {
                printer.InTrucTiep(this);
            }
        }
        #endregion

        #region 2. TAB TÍNH TỒN KHO QUA STORED PROCEDURE
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

            // Thẻ KPI tổng kết
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

            // Gán sự kiện
            btnTinhTonKho.Click += (s, e) => TaiBaoCaoTonKho();
            btnLamMoiTon.Click += (s, e) =>
            {
                cboLoaiHang.SelectedIndex = 0;
                cboTrangThaiTon.SelectedIndex = 0;
                txtTuKhoaTon.Clear();
                TaiBaoCaoTonKho();
            };
            btnXuatCsvTonKho.Click += (s, e) => CsvHelperCustom.XuatDataGridViewRaCsv(dgvTonKho, "BaoCaoTonKho", this);

            // Tô màu cảnh báo tồn thấp / hết hàng
            dgvTonKho.CellFormatting += DgvTonKho_CellFormatting;
        }

        private void NapDanhMucLoaiHang()
        {
            try
            {
                DataTable dt = Database.GetData("SELECT MaLoai, TenLoai FROM LoaiHang ORDER BY TenLoai");
                DataRow rAll = dt.NewRow();
                rAll["MaLoai"] = "";
                rAll["TenLoai"] = "-- Tất cả loại hàng --";
                dt.Rows.InsertAt(rAll, 0);

                cboLoaiHang.DisplayMember = "TenLoai";
                cboLoaiHang.ValueMember = "MaLoai";
                cboLoaiHang.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục loại hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TaiBaoCaoTonKho()
        {
            try
            {
                string maLoai = cboLoaiHang.SelectedValue != null ? cboLoaiHang.SelectedValue.ToString() : "";
                string trangThai = cboTrangThaiTon.SelectedItem != null ? cboTrangThaiTon.SelectedItem.ToString() : "Tất cả";
                string tuKhoa = txtTuKhoaTon.Text.Trim();

                DataTable dt;

                // Thử gọi Stored Procedure sp_TinhTonKhoChiTiet nếu đã thực thi script, nếu chưa có thì fallback câu SELECT chuẩn
                try
                {
                    dt = Database.GetDataProc("sp_TinhTonKhoChiTiet",
                        Database.Param("@MaLoai", maLoai),
                        Database.Param("@TuKhoa", tuKhoa),
                        Database.Param("@TrangThaiTon", trangThai));
                }
                catch
                {
                    // Fallback SQL nếu database chưa kịp chạy script mới
                    string sqlFallback = @"
                        WITH BangThongKe AS (
                            SELECT 
                                h.MaHang,
                                h.TenHang,
                                h.MaLoai,
                                l.TenLoai,
                                h.DonViTinh,
                                h.DonGiaNhap,
                                h.SoLuongTon,
                                h.MucTonToiThieu,
                                CAST(h.SoLuongTon * h.DonGiaNhap AS DECIMAL(18,2)) AS GiaTriTon,
                                ISNULL((
                                    SELECT SUM(ct.SoLuong)
                                    FROM ChiTietPhieu ct
                                    INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                                    WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Nhập'
                                ), 0) AS TongNhap,
                                ISNULL((
                                    SELECT SUM(ct.SoLuong)
                                    FROM ChiTietPhieu ct
                                    INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                                    WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Xuất'
                                ), 0) AS TongXuat,
                                CASE 
                                    WHEN h.SoLuongTon = 0 THEN N'Hết hàng'
                                    WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
                                    ELSE N'An toàn'
                                END AS TrangThai
                            FROM HangHoa h
                            INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
                            WHERE (@MaLoai = '' OR h.MaLoai = @MaLoai)
                              AND (@TuKhoa = N'' OR h.MaHang LIKE '%' + @TuKhoa + '%' OR h.TenHang LIKE N'%' + @TuKhoa + '%')
                        )
                        SELECT * FROM BangThongKe
                        WHERE (@TrangThaiTon = N'Tất cả' OR TrangThai = @TrangThaiTon)
                        ORDER BY 
                            CASE TrangThai WHEN N'Hết hàng' THEN 1 WHEN N'Cảnh báo tồn thấp' THEN 2 ELSE 3 END,
                            MaHang ASC";

                    dt = Database.GetData(sqlFallback,
                        Database.Param("@MaLoai", maLoai),
                        Database.Param("@TuKhoa", tuKhoa),
                        Database.Param("@TrangThaiTon", trangThai));
                }

                dgvTonKho.DataSource = dt;

                // Định dạng cột hiển thị
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

                // Thống kê thẻ KPI
                decimal tongGiaTri = 0;
                int soCanhBao = 0;
                foreach (DataRow r in dt.Rows)
                {
                    tongGiaTri += Convert.ToDecimal(r["GiaTriTon"]);
                    string tt = r["TrangThai"].ToString();
                    if (tt == "Cảnh báo tồn thấp" || tt == "Hết hàng") soCanhBao++;
                }

                lblTongMatHang.Text = "Số mặt hàng: " + dt.Rows.Count;
                lblTongGiaTriTon.Text = "Tổng giá trị tồn: " + tongGiaTri.ToString("#,##0") + " VNĐ";
                lblSoHangCanhBao.Text = "Hàng cảnh báo / hết: " + soCanhBao;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tính tồn kho: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 226, 226); // Đỏ nhạt
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
                }
                else if (tt == "Cảnh báo tồn thấp")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199); // Vàng cam nhạt
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                }
            }
        }
        #endregion

        #region 3. TAB VẼ BIỂU ĐỒ GDI+
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
                Value = DateTime.Today.AddMonths(-2) // 2 tháng trước
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

            // Panel vẽ đồ thị GDI+ có bật DoubleBuffered
            pnlCanvas = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            pnlCanvas.Paint += PnlCanvas_Paint;

            tabBieuDo.Controls.Add(pnlCanvas);
            tabBieuDo.Controls.Add(pnlTop);

            btnVeBieuDo.Click += (s, e) => TaiDuLieuBieuDo();
            radCot.CheckedChanged += (s, e) => { if (radCot.Checked) pnlCanvas.Invalidate(); };
            radDuong.CheckedChanged += (s, e) => { if (radDuong.Checked) pnlCanvas.Invalidate(); };
            btnLuuAnhBieuDo.Click += (s, e) =>
            {
                BieuDoGdiHelper.LoaiBieuDo loai = radCot.Checked ? BieuDoGdiHelper.LoaiBieuDo.CotNhom : BieuDoGdiHelper.LoaiBieuDo.Duong;
                BieuDoGdiHelper.LuuAnhBieuDo(duLieuBieuDo, loai, this);
            };
            btnXuatCsvBieuDo.Click += (s, e) =>
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Mã hàng");
                dt.Columns.Add("Tên hàng hóa");
                dt.Columns.Add("Tổng nhập", typeof(int));
                dt.Columns.Add("Tổng xuất", typeof(int));
                dt.Columns.Add("Tồn hiện tại", typeof(int));

                foreach (var item in duLieuBieuDo)
                {
                    dt.Rows.Add(item.MaHang, item.TenHang, item.TongNhap, item.TongXuat, item.TonHienTai);
                }

                CsvHelperCustom.XuatDataTableRaCsv(dt, "SoLieuBieuDoNhapXuatTon", this);
            };
        }

        private void TaiDuLieuBieuDo()
        {
            try
            {
                DateTime tuNgay = dtpBieuDoTu.Value.Date;
                DateTime denNgayChuan = dtpBieuDoDen.Value.Date.AddDays(1);

                // Dùng stored procedure sp_ThongKeNhapXuatTon có sẵn trong database hoặc câu truy vấn tương đương
                DataTable dt;
                try
                {
                    dt = Database.GetDataProc("sp_ThongKeNhapXuatTon",
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgay", denNgayChuan));
                }
                catch
                {
                    string sqlFallback = @"
                        SELECT 
                            h.MaHang,
                            h.TenHang,
                            ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Nhập' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongNhap,
                            ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Xuất' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongXuat,
                            h.SoLuongTon AS TonHienTai
                        FROM HangHoa h
                        LEFT JOIN ChiTietPhieu ct ON h.MaHang = ct.MaHang
                        LEFT JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                        GROUP BY h.MaHang, h.TenHang, h.SoLuongTon
                        ORDER BY h.MaHang ASC";

                    dt = Database.GetData(sqlFallback,
                        Database.Param("@TuNgay", tuNgay),
                        Database.Param("@DenNgay", denNgayChuan));
                }

                duLieuBieuDo = BieuDoGdiHelper.ChuyenDoiDataTable(dt);
                pnlCanvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp số liệu biểu đồ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            BieuDoGdiHelper.LoaiBieuDo loai = radCot.Checked ? BieuDoGdiHelper.LoaiBieuDo.CotNhom : BieuDoGdiHelper.LoaiBieuDo.Duong;
            string tieuDe = string.Format("BIỂU ĐỒ SO SÁNH NHẬP - XUẤT - TỒN (Từ {0:dd/MM/yyyy} đến {1:dd/MM/yyyy})", dtpBieuDoTu.Value, dtpBieuDoDen.Value);
            BieuDoGdiHelper.VeBieuDo(e.Graphics, pnlCanvas.ClientRectangle, duLieuBieuDo, loai, tieuDe);
        }
        #endregion

        #region UTILITIES TẠO GIAO DIỆN CHUẨN
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

        // Custom Panel tự động kích hoạt DoubleBuffered
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
