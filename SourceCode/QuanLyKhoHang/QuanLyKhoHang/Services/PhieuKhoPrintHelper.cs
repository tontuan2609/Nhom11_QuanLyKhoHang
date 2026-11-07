using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace QuanLyKhoHang.Services
{
    /// <summary>
    /// Service in phiếu kho bằng PrintDocument, hỗ trợ PrintPreview và chuyển đổi số tiền thành chữ tiếng Việt
    /// </summary>
    public class PhieuKhoPrintHelper
    {
        private readonly string maPhieu;
        private DataRow thongTinPhieu;
        private DataTable chiTietPhieu;
        private int dongHienTai = 0;
        private int soTrang = 1;

        public PhieuKhoPrintHelper(string maPhieu)
        {
            this.maPhieu = maPhieu;
        }

        private bool TaiDuLieu()
        {
            try
            {
                string sqlHeader = @"
                    SELECT 
                        p.MaPhieu,
                        p.LoaiPhieu,
                        p.NgayLap,
                        p.MaNguoiDung,
                        nd.HoTen AS NguoiLap,
                        p.MaNCC,
                        ISNULL(ncc.TenNCC, N'(Không có / Khách hàng)') AS TenNCC,
                        ISNULL(ncc.DiaChi, N'-') AS DiaChiNCC,
                        ISNULL(ncc.DienThoai, N'-') AS DienThoaiNCC,
                        ISNULL(p.GhiChu, N'-') AS GhiChu,
                        p.TongTien
                    FROM PhieuKho p
                    INNER JOIN NguoiDung nd ON p.MaNguoiDung = nd.MaNguoiDung
                    LEFT JOIN NhaCungCap ncc ON p.MaNCC = ncc.MaNCC
                    WHERE p.MaPhieu = @MaPhieu";

                DataTable dtHeader = Database.GetData(sqlHeader, Database.Param("@MaPhieu", maPhieu));
                if (dtHeader.Rows.Count == 0)
                {
                    return false;
                }
                thongTinPhieu = dtHeader.Rows[0];

                string sqlChiTiet = @"
                    SELECT 
                        ct.MaHang,
                        h.TenHang,
                        h.DonViTinh,
                        ct.SoLuong,
                        ct.DonGia,
                        ct.ThanhTien
                    FROM ChiTietPhieu ct
                    INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
                    WHERE ct.MaPhieu = @MaPhieu
                    ORDER BY ct.MaHang ASC";

                chiTietPhieu = Database.GetData(sqlChiTiet, Database.Param("@MaPhieu", maPhieu));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu phiếu in: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void XemTruoc(IWin32Window parent)
        {
            if (!TaiDuLieu())
            {
                MessageBox.Show("Không tìm thấy thông tin phiếu kho mã: " + maPhieu, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dongHienTai = 0;
            soTrang = 1;

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DocumentName = "In_PhieuKho_" + maPhieu;
                pd.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
                pd.PrintPage += InTrangPhieu;

                using (PrintPreviewDialog ppd = new PrintPreviewDialog())
                {
                    ppd.Document = pd;
                    ppd.Width = 900;
                    ppd.Height = 700;
                    ppd.StartPosition = FormStartPosition.CenterParent;
                    ppd.Text = "Xem trước bản in: " + maPhieu;
                    ppd.ShowDialog(parent);
                }
            }
        }

        public void InTrucTiep(IWin32Window parent)
        {
            if (!TaiDuLieu())
            {
                MessageBox.Show("Không tìm thấy thông tin phiếu kho mã: " + maPhieu, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dongHienTai = 0;
            soTrang = 1;

            using (PrintDocument pd = new PrintDocument())
            {
                pd.DocumentName = "In_PhieuKho_" + maPhieu;
                pd.DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);
                pd.PrintPage += InTrangPhieu;

                using (PrintDialog pDialog = new PrintDialog())
                {
                    pDialog.Document = pd;
                    pDialog.UseEXDialog = true;
                    if (pDialog.ShowDialog(parent) == DialogResult.OK)
                    {
                        pd.Print();
                    }
                }
            }
        }

        private void InTrangPhieu(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font fontHeader = new Font("Arial", 16, FontStyle.Bold);
            Font fontSubHeader = new Font("Arial", 10, FontStyle.Bold);
            Font fontRegular = new Font("Arial", 9, FontStyle.Regular);
            Font fontBold = new Font("Arial", 9, FontStyle.Bold);
            Font fontItalic = new Font("Arial", 9, FontStyle.Italic);

            Brush brushBlack = Brushes.Black;
            Pen penBorder = new Pen(Color.Black, 1);
            Pen penDashed = new Pen(Color.Gray, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

            int left = e.MarginBounds.Left;
            int right = e.MarginBounds.Right;
            int top = e.MarginBounds.Top;
            int bottom = e.MarginBounds.Bottom;
            int width = right - left;

            int y = top;

            // Header Đơn vị / Doanh nghiệp
            g.DrawString("CÔNG TY CỔ PHẦN CÔNG NGHỆ VÀ THIẾT BỊ SỐ", fontSubHeader, brushBlack, left, y);
            y += 20;
            g.DrawString("Địa chỉ: 123 Đường Nguyễn Văn Cừ, Quận 5, TP. Hồ Chí Minh", fontRegular, brushBlack, left, y);
            y += 18;
            g.DrawString("Hotline: 028.3835.4400 - Email: khohang@nhom11.vn", fontRegular, brushBlack, left, y);

            string maPhieuStr = "Số phiếu: " + maPhieu;
            SizeF szMaPhieu = g.MeasureString(maPhieuStr, fontBold);
            g.DrawString(maPhieuStr, fontBold, brushBlack, right - szMaPhieu.Width, top);
            string mauSo = "(Mẫu số 01 - VT ban hành theo TT 200/2014/TT-BTC)";
            SizeF szMauSo = g.MeasureString(mauSo, fontItalic);
            g.DrawString(mauSo, fontItalic, brushBlack, right - szMauSo.Width, top + 20);

            y += 25;
            g.DrawLine(penDashed, left, y, right, y);
            y += 15;

            string loaiPhieu = thongTinPhieu["LoaiPhieu"].ToString();
            string tieuDePhieu = (loaiPhieu == "Nhập") ? "PHIẾU NHẬP KHO" : "PHIẾU XUẤT KHO";
            SizeF szTieuDe = g.MeasureString(tieuDePhieu, fontHeader);
            g.DrawString(tieuDePhieu, fontHeader, brushBlack, left + (width - szTieuDe.Width) / 2, y);
            y += (int)szTieuDe.Height + 5;

            DateTime ngayLap = Convert.ToDateTime(thongTinPhieu["NgayLap"]);
            string ngayLapStr = string.Format("Ngày {0:dd} tháng {0:MM} năm {0:yyyy} (Giờ lập: {0:HH:mm})", ngayLap);
            SizeF szNgay = g.MeasureString(ngayLapStr, fontItalic);
            g.DrawString(ngayLapStr, fontItalic, brushBlack, left + (width - szNgay.Width) / 2, y);
            y += (int)szNgay.Height + 15;

            string labelDoiTac = (loaiPhieu == "Nhập") ? "Nhà cung cấp: " : "Đơn vị nhận / Khách: ";
            g.DrawString("- Người lập phiếu: " + thongTinPhieu["NguoiLap"].ToString(), fontRegular, brushBlack, left, y);
            g.DrawString("- " + labelDoiTac + thongTinPhieu["TenNCC"].ToString(), fontRegular, brushBlack, left + width / 2, y);
            y += 22;

            g.DrawString("- Địa chỉ đối tác: " + thongTinPhieu["DiaChiNCC"].ToString(), fontRegular, brushBlack, left, y);
            g.DrawString("- Điện thoại: " + thongTinPhieu["DienThoaiNCC"].ToString(), fontRegular, brushBlack, left + width / 2, y);
            y += 22;

            g.DrawString("- Diễn giải / Ghi chú: " + thongTinPhieu["GhiChu"].ToString(), fontRegular, brushBlack, left, y);
            y += 28;

            int[] colWidths = new int[] { 40, 80, 220, 60, 60, 110, 140 };
            string[] colHeaders = new string[] { "STT", "Mã hàng", "Tên hàng hóa", "ĐVT", "Số lượng", "Đơn giá (VNĐ)", "Thành tiền (VNĐ)" };

            int totalColW = 0;
            for (int i = 0; i < colWidths.Length; i++) totalColW += colWidths[i];
            float tiLe = (float)width / totalColW;
            int[] cols = new int[colWidths.Length];
            for (int i = 0; i < colWidths.Length; i++) cols[i] = (int)(colWidths[i] * tiLe);

            int headerH = 28;
            g.FillRectangle(Brushes.LightGray, left, y, width, headerH);
            g.DrawRectangle(penBorder, left, y, width, headerH);

            int curX = left;
            for (int i = 0; i < cols.Length; i++)
            {
                g.DrawRectangle(penBorder, curX, y, cols[i], headerH);
                StringFormat sf = new StringFormat
                {
                    Alignment = (i == 0 || i == 3) ? StringAlignment.Center : (i >= 4 ? StringAlignment.Far : StringAlignment.Near),
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString(colHeaders[i], fontBold, brushBlack, new RectangleF(curX + 3, y, cols[i] - 6, headerH), sf);
                curX += cols[i];
            }
            y += headerH;

            int stt = dongHienTai + 1;
            int rowH = 24;

            while (dongHienTai < chiTietPhieu.Rows.Count)
            {
                if (y + rowH + 160 > bottom)
                {
                    e.HasMorePages = true;
                    soTrang++;
                    return;
                }

                DataRow r = chiTietPhieu.Rows[dongHienTai];
                curX = left;

                g.DrawRectangle(penBorder, left, y, width, rowH);

                string[] rowData = new string[]
                {
                    stt.ToString(),
                    r["MaHang"].ToString(),
                    r["TenHang"].ToString(),
                    r["DonViTinh"].ToString(),
                    Convert.ToInt32(r["SoLuong"]).ToString("N0"),
                    Convert.ToDecimal(r["DonGia"]).ToString("#,##0"),
                    Convert.ToDecimal(r["ThanhTien"]).ToString("#,##0")
                };

                for (int i = 0; i < cols.Length; i++)
                {
                    g.DrawRectangle(penBorder, curX, y, cols[i], rowH);
                    StringFormat sf = new StringFormat
                    {
                        Alignment = (i == 0 || i == 3) ? StringAlignment.Center : (i >= 4 ? StringAlignment.Far : StringAlignment.Near),
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(rowData[i], fontRegular, brushBlack, new RectangleF(curX + 3, y, cols[i] - 6, rowH), sf);
                    curX += cols[i];
                }

                y += rowH;
                stt++;
                dongHienTai++;
            }

            decimal tongTien = Convert.ToDecimal(thongTinPhieu["TongTien"]);
            g.FillRectangle(new SolidBrush(Color.FromArgb(245, 245, 245)), left, y, width, rowH);
            g.DrawRectangle(penBorder, left, y, width, rowH);

            int cotTruocTong = cols[0] + cols[1] + cols[2] + cols[3] + cols[4] + cols[5];
            g.DrawString("TỔNG TIỀN THANH TOÁN:", fontBold, brushBlack, new RectangleF(left + 10, y, cotTruocTong - 20, rowH), new StringFormat { LineAlignment = StringAlignment.Center });
            g.DrawString(tongTien.ToString("#,##0") + " VNĐ", fontBold, brushBlack, new RectangleF(left + cotTruocTong, y, cols[6] - 6, rowH), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
            y += rowH + 10;

            string bangChu = DocTienBangChu(tongTien);
            g.DrawString("- Số tiền viết bằng chữ: " + bangChu, fontItalic, brushBlack, left, y);
            y += 30;

            int blockW = width / 3;
            StringFormat sfSign = new StringFormat { Alignment = StringAlignment.Center };

            g.DrawString("Người lập phiếu", fontBold, brushBlack, new RectangleF(left, y, blockW, 20), sfSign);
            g.DrawString("(Ký, ghi rõ họ tên)", fontItalic, brushBlack, new RectangleF(left, y + 18, blockW, 20), sfSign);
            g.DrawString(thongTinPhieu["NguoiLap"].ToString(), fontBold, brushBlack, new RectangleF(left, y + 80, blockW, 20), sfSign);

            string roleSign = (loaiPhieu == "Nhập") ? "Người giao hàng" : "Người nhận hàng";
            g.DrawString(roleSign, fontBold, brushBlack, new RectangleF(left + blockW, y, blockW, 20), sfSign);
            g.DrawString("(Ký, ghi rõ họ tên)", fontItalic, brushBlack, new RectangleF(left + blockW, y + 18, blockW, 20), sfSign);

            g.DrawString("Thủ kho / Kế toán", fontBold, brushBlack, new RectangleF(left + blockW * 2, y, blockW, 20), sfSign);
            g.DrawString("(Ký, đóng dấu)", fontItalic, brushBlack, new RectangleF(left + blockW * 2, y + 18, blockW, 20), sfSign);

            e.HasMorePages = false;
        }

        public static string DocTienBangChu(decimal soTien)
        {
            if (soTien == 0) return "Không đồng.";
            if (soTien < 0) return "Âm " + DocTienBangChu(Math.Abs(soTien));

            long s = (long)Math.Round(soTien);
            string[] hangDonVi = { "", "nghìn", "triệu", "tỷ", "nghìn tỷ", "triệu tỷ" };

            string ketQua = "";
            int cap = 0;

            while (s > 0)
            {
                long baSo = s % 1000;
                if (baSo > 0)
                {
                    string chuoiBaSo = DocCumBaSo((int)baSo, s >= 1000);
                    ketQua = chuoiBaSo + " " + hangDonVi[cap] + " " + ketQua;
                }
                s /= 1000;
                cap++;
            }

            ketQua = ketQua.Trim();
            if (ketQua.Length > 0)
            {
                ketQua = char.ToUpper(ketQua[0]) + ketQua.Substring(1) + " đồng chẵn.";
            }

            return ketQua;
        }

        private static string DocCumBaSo(int n, bool coHangTruoc)
        {
            string[] chuSo = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            int tram = n / 100;
            int chuc = (n % 100) / 10;
            int donVi = n % 10;

            string s = "";

            if (tram > 0 || coHangTruoc)
            {
                s += chuSo[tram] + " trăm ";
            }

            if (chuc > 1)
            {
                s += chuSo[chuc] + " mươi ";
                if (donVi == 1) s += "mốt";
                else if (donVi == 5) s += "lăm";
                else if (donVi > 0) s += chuSo[donVi];
            }
            else if (chuc == 1)
            {
                s += "mười ";
                if (donVi == 5) s += "lăm";
                else if (donVi > 0) s += chuSo[donVi];
            }
            else
            {
                if ((tram > 0 || coHangTruoc) && donVi > 0)
                {
                    s += "lẻ " + chuSo[donVi];
                }
                else if (donVi > 0)
                {
                    s += chuSo[donVi];
                }
            }

            return s.Trim();
        }
    }
}
