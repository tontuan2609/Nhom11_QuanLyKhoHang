using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public class FrmMain : Form
    {
        private MenuStrip menu;
        private StatusStrip thanhTrangThai;
        private ToolStripStatusLabel lblNguoiDung;
        private ToolStripStatusLabel lblThoiGian;
        private ToolStripMenuItem mnuBaoCao;
        private Timer dongHo;

        public bool DangXuat { get; private set; }

        public FrmMain()
        {
            Text = "Quản lý kho hàng";
            IsMdiContainer = true;
            WindowState = FormWindowState.Maximized;

            KhoiTaoMenu();
            KhoiTaoThanhTrangThai();
            PhanQuyen();
        }

        public void MoForm<T>() where T : Form, new()
        {
            foreach (Form con in MdiChildren)
            {
                if (con is T)
                {
                    con.Activate();
                    return;
                }
            }

            T form = new T();
            form.MdiParent = this;
            form.Show();
        }

        private void KhoiTaoMenu()
        {
            menu = new MenuStrip();

            ToolStripMenuItem mnuHeThong = new ToolStripMenuItem("&Hệ thống");
            ToolStripMenuItem mnuDangXuat = new ToolStripMenuItem("Đăng &xuất") { ShortcutKeys = Keys.Control | Keys.L };
            ToolStripMenuItem mnuThoat = new ToolStripMenuItem("&Thoát");
            mnuDangXuat.Click += MnuDangXuat_Click;
            mnuThoat.Click += (s, e) => Close();
            mnuHeThong.DropDownItems.Add(mnuDangXuat);
            mnuHeThong.DropDownItems.Add(new ToolStripSeparator());
            mnuHeThong.DropDownItems.Add(mnuThoat);

            ToolStripMenuItem mnuDanhMuc = new ToolStripMenuItem("&Danh mục");
            ToolStripMenuItem mnuLoaiHang = new ToolStripMenuItem("&Loại hàng");
            ToolStripMenuItem mnuHangHoa = new ToolStripMenuItem("&Hàng hóa");
            mnuLoaiHang.Click += (s, e) => MoForm<FrmLoaiHang>();
            mnuHangHoa.Click += (s, e) => MoForm<FrmHangHoa>();
            mnuDanhMuc.DropDownItems.Add(mnuLoaiHang);
            mnuDanhMuc.DropDownItems.Add(mnuHangHoa);

            ToolStripMenuItem mnuNghiepVu = new ToolStripMenuItem("&Nghiệp vụ kho");
            ToolStripMenuItem mnuNhapKho = new ToolStripMenuItem("Nhập kho");
            ToolStripMenuItem mnuXuatKho = new ToolStripMenuItem("Xuất kho");
            mnuNhapKho.Click += (s, e) => ChucNangChuaCo();
            mnuXuatKho.Click += (s, e) => ChucNangChuaCo();
            mnuNghiepVu.DropDownItems.Add(mnuNhapKho);
            mnuNghiepVu.DropDownItems.Add(mnuXuatKho);

            mnuBaoCao = new ToolStripMenuItem("&Báo cáo");
            ToolStripMenuItem mnuThongKe = new ToolStripMenuItem("Thống kê nhập - xuất - tồn");
            mnuThongKe.Click += (s, e) => ChucNangChuaCo();
            mnuBaoCao.DropDownItems.Add(mnuThongKe);

            ToolStripMenuItem mnuCuaSo = new ToolStripMenuItem("&Cửa sổ");
            ToolStripMenuItem mnuXepTang = new ToolStripMenuItem("Xếp tầng");
            ToolStripMenuItem mnuXepNgang = new ToolStripMenuItem("Xếp ngang");
            ToolStripMenuItem mnuDongTatCa = new ToolStripMenuItem("Đóng tất cả");
            mnuXepTang.Click += (s, e) => LayoutMdi(MdiLayout.Cascade);
            mnuXepNgang.Click += (s, e) => LayoutMdi(MdiLayout.TileHorizontal);
            mnuDongTatCa.Click += (s, e) =>
            {
                foreach (Form con in MdiChildren)
                {
                    con.Close();
                }
            };
            mnuCuaSo.DropDownItems.Add(mnuXepTang);
            mnuCuaSo.DropDownItems.Add(mnuXepNgang);
            mnuCuaSo.DropDownItems.Add(mnuDongTatCa);

            menu.Items.Add(mnuHeThong);
            menu.Items.Add(mnuDanhMuc);
            menu.Items.Add(mnuNghiepVu);
            menu.Items.Add(mnuBaoCao);
            menu.Items.Add(mnuCuaSo);
            menu.MdiWindowListItem = mnuCuaSo;

            MainMenuStrip = menu;
            Controls.Add(menu);
        }

        private void KhoiTaoThanhTrangThai()
        {
            thanhTrangThai = new StatusStrip();
            lblNguoiDung = new ToolStripStatusLabel
            {
                Text = "Người dùng: " + PhienDangNhap.HoTen + " (" + PhienDangNhap.VaiTro + ")",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            lblThoiGian = new ToolStripStatusLabel(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            thanhTrangThai.Items.Add(lblNguoiDung);
            thanhTrangThai.Items.Add(lblThoiGian);
            Controls.Add(thanhTrangThai);

            dongHo = new Timer { Interval = 1000 };
            dongHo.Tick += (s, e) => lblThoiGian.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            dongHo.Start();
        }

        private void PhanQuyen()
        {
            mnuBaoCao.Enabled = PhienDangNhap.LaQuanLy;
        }

        private void ChucNangChuaCo()
        {
            UiHelper.HienThongBao("Chức năng này do thành viên khác phụ trách, chưa được tích hợp.");
        }

        private void MnuDangXuat_Click(object sender, EventArgs e)
        {
            if (UiHelper.XacNhan("Bạn có chắc muốn đăng xuất?"))
            {
                DangXuat = true;
                Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!DangXuat && e.CloseReason == CloseReason.UserClosing && !UiHelper.XacNhan("Bạn có chắc muốn thoát chương trình?"))
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            dongHo.Stop();
            dongHo.Dispose();
            base.OnFormClosed(e);
        }
    }
}
