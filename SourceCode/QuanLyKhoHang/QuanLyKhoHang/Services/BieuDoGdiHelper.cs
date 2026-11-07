using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace QuanLyKhoHang.Services
{
    /// <summary>
    /// Service vẽ biểu đồ nhập - xuất - tồn bằng GDI+ (System.Drawing)
    /// Hỗ trợ cả Biểu đồ Cột nhóm Gradient và Biểu đồ Đường xu hướng
    /// </summary>
    public class BieuDoGdiHelper
    {
        public enum LoaiBieuDo
        {
            CotNhom,
            Duong
        }

        public class ItemThongKe
        {
            public string MaHang { get; set; }
            public string TenHang { get; set; }
            public int TongNhap { get; set; }
            public int TongXuat { get; set; }
            public int TonHienTai { get; set; }
        }

        public static List<ItemThongKe> ChuyenDoiDataTable(DataTable dt)
        {
            List<ItemThongKe> list = new List<ItemThongKe>();
            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                ItemThongKe item = new ItemThongKe
                {
                    MaHang = row["MaHang"].ToString(),
                    TenHang = row["TenHang"].ToString(),
                    TongNhap = Convert.ToInt32(row["TongNhap"]),
                    TongXuat = Convert.ToInt32(row["TongXuat"]),
                    TonHienTai = Convert.ToInt32(row["TonHienTai"])
                };
                list.Add(item);
            }
            return list;
        }

        public static void VeBieuDo(
            Graphics g,
            Rectangle bounds,
            List<ItemThongKe> duLieu,
            LoaiBieuDo loai = LoaiBieuDo.CotNhom,
            string tieuDe = "BIỂU ĐỒ SO SÁNH NHẬP - XUẤT - TỒN KHO")
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.FillRectangle(Brushes.White, bounds);

            Font fontTieuDe = new Font("Segoe UI", 12, FontStyle.Bold);
            Font fontLegend = new Font("Segoe UI", 9, FontStyle.Bold);
            Font fontNhan = new Font("Segoe UI", 8, FontStyle.Regular);
            Font fontSo = new Font("Segoe UI", 8, FontStyle.Bold);

            // Tiêu đề
            SizeF szTieuDe = g.MeasureString(tieuDe, fontTieuDe);
            g.DrawString(tieuDe, fontTieuDe, new SolidBrush(Color.FromArgb(30, 41, 59)),
                bounds.Left + (bounds.Width - szTieuDe.Width) / 2, bounds.Top + 10);

            // Chú giải
            int legendY = bounds.Top + 38;
            int boxSize = 14;
            Color colNhap = Color.FromArgb(37, 99, 235);
            Color colXuat = Color.FromArgb(220, 38, 38);
            Color colTon = Color.FromArgb(22, 163, 74);

            int legendX = bounds.Left + (bounds.Width - 360) / 2;

            g.FillRectangle(new SolidBrush(colNhap), legendX, legendY, boxSize, boxSize);
            g.DrawRectangle(Pens.DarkBlue, legendX, legendY, boxSize, boxSize);
            g.DrawString("Tổng nhập", fontLegend, Brushes.Black, legendX + boxSize + 6, legendY);

            legendX += 120;
            g.FillRectangle(new SolidBrush(colXuat), legendX, legendY, boxSize, boxSize);
            g.DrawRectangle(Pens.DarkRed, legendX, legendY, boxSize, boxSize);
            g.DrawString("Tổng xuất", fontLegend, Brushes.Black, legendX + boxSize + 6, legendY);

            legendX += 120;
            g.FillRectangle(new SolidBrush(colTon), legendX, legendY, boxSize, boxSize);
            g.DrawRectangle(Pens.DarkGreen, legendX, legendY, boxSize, boxSize);
            g.DrawString("Tồn hiện tại", fontLegend, Brushes.Black, legendX + boxSize + 6, legendY);

            if (duLieu == null || duLieu.Count == 0)
            {
                string msgKhongDuLieu = "Không có dữ liệu phát sinh trong khoảng thời gian này.";
                SizeF szMsg = g.MeasureString(msgKhongDuLieu, fontLegend);
                g.DrawString(msgKhongDuLieu, fontLegend, Brushes.Gray,
                    bounds.Left + (bounds.Width - szMsg.Width) / 2, bounds.Top + bounds.Height / 2);
                return;
            }

            int padLeft = 65;
            int padRight = 30;
            int padTop = 75;
            int padBottom = 70;

            Rectangle plotArea = new Rectangle(
                bounds.Left + padLeft,
                bounds.Top + padTop,
                bounds.Width - padLeft - padRight,
                bounds.Height - padTop - padBottom
            );

            if (plotArea.Width <= 10 || plotArea.Height <= 10) return;

            int maxVal = 10;
            foreach (var item in duLieu)
            {
                if (item.TongNhap > maxVal) maxVal = item.TongNhap;
                if (item.TongXuat > maxVal) maxVal = item.TongXuat;
                if (item.TonHienTai > maxVal) maxVal = item.TonHienTai;
            }

            int step = TinhStepPhuHop(maxVal, 5);
            int yMax = step * 5;
            if (yMax < maxVal) yMax += step;

            Pen penGrid = new Pen(Color.FromArgb(226, 232, 240), 1) { DashStyle = DashStyle.Dash };
            Pen penAxis = new Pen(Color.FromArgb(100, 116, 139), 1.5f);
            Brush brushText = new SolidBrush(Color.FromArgb(71, 85, 105));

            int soVach = 5;
            for (int i = 0; i <= soVach; i++)
            {
                int val = i * (yMax / soVach);
                float yPos = plotArea.Bottom - (float)val / yMax * plotArea.Height;

                if (i > 0)
                {
                    g.DrawLine(penGrid, plotArea.Left, yPos, plotArea.Right, yPos);
                }

                string labelVal = val.ToString("N0");
                SizeF szLabel = g.MeasureString(labelVal, fontNhan);
                g.DrawString(labelVal, fontNhan, brushText, plotArea.Left - szLabel.Width - 6, yPos - szLabel.Height / 2);
            }

            g.DrawLine(penAxis, plotArea.Left, plotArea.Top, plotArea.Left, plotArea.Bottom);
            g.DrawLine(penAxis, plotArea.Left, plotArea.Bottom, plotArea.Right, plotArea.Bottom);
            g.DrawString("(Số lượng)", fontNhan, brushText, plotArea.Left - 50, plotArea.Top - 18);

            int n = duLieu.Count;
            float slotWidth = (float)plotArea.Width / n;

            if (loai == LoaiBieuDo.CotNhom)
            {
                float barWidth = Math.Max(6, Math.Min(26, (slotWidth - 20) / 3));

                for (int i = 0; i < n; i++)
                {
                    var it = duLieu[i];
                    float centerX = plotArea.Left + i * slotWidth + slotWidth / 2;

                    float xNhap = centerX - barWidth * 1.5f;
                    float xXuat = centerX - barWidth * 0.5f;
                    float xTon = centerX + barWidth * 0.5f;

                    float hNhap = (float)it.TongNhap / yMax * plotArea.Height;
                    float hXuat = (float)it.TongXuat / yMax * plotArea.Height;
                    float hTon = (float)it.TonHienTai / yMax * plotArea.Height;

                    VeCotGradient(g, xNhap, plotArea.Bottom - hNhap, barWidth, hNhap, colNhap, Color.FromArgb(147, 197, 253));
                    if (it.TongNhap > 0)
                    {
                        VeGiaTriTrenCot(g, it.TongNhap.ToString(), fontSo, Brushes.DarkBlue, xNhap + barWidth / 2, plotArea.Bottom - hNhap);
                    }

                    VeCotGradient(g, xXuat, plotArea.Bottom - hXuat, barWidth, hXuat, colXuat, Color.FromArgb(252, 165, 165));
                    if (it.TongXuat > 0)
                    {
                        VeGiaTriTrenCot(g, it.TongXuat.ToString(), fontSo, Brushes.DarkRed, xXuat + barWidth / 2, plotArea.Bottom - hXuat);
                    }

                    VeCotGradient(g, xTon, plotArea.Bottom - hTon, barWidth, hTon, colTon, Color.FromArgb(134, 239, 172));
                    if (it.TonHienTai > 0)
                    {
                        VeGiaTriTrenCot(g, it.TonHienTai.ToString(), fontSo, Brushes.DarkGreen, xTon + barWidth / 2, plotArea.Bottom - hTon);
                    }

                    VeNhanTrucX(g, it.TenHang, it.MaHang, fontNhan, brushText, centerX, plotArea.Bottom + 6, slotWidth - 6);
                }
            }
            else
            {
                PointF[] ptsNhap = new PointF[n];
                PointF[] ptsXuat = new PointF[n];
                PointF[] ptsTon = new PointF[n];

                for (int i = 0; i < n; i++)
                {
                    var it = duLieu[i];
                    float cx = plotArea.Left + i * slotWidth + slotWidth / 2;
                    ptsNhap[i] = new PointF(cx, plotArea.Bottom - (float)it.TongNhap / yMax * plotArea.Height);
                    ptsXuat[i] = new PointF(cx, plotArea.Bottom - (float)it.TongXuat / yMax * plotArea.Height);
                    ptsTon[i] = new PointF(cx, plotArea.Bottom - (float)it.TonHienTai / yMax * plotArea.Height);

                    VeNhanTrucX(g, it.TenHang, it.MaHang, fontNhan, brushText, cx, plotArea.Bottom + 6, slotWidth - 6);
                }

                using (Pen penLineNhap = new Pen(colNhap, 2.5f))
                using (Pen penLineXuat = new Pen(colXuat, 2.5f))
                using (Pen penLineTon = new Pen(colTon, 2.5f))
                {
                    if (n > 1)
                    {
                        g.DrawLines(penLineNhap, ptsNhap);
                        g.DrawLines(penLineXuat, ptsXuat);
                        g.DrawLines(penLineTon, ptsTon);
                    }

                    int dotR = 4;
                    for (int i = 0; i < n; i++)
                    {
                        g.FillEllipse(new SolidBrush(colNhap), ptsNhap[i].X - dotR, ptsNhap[i].Y - dotR, dotR * 2, dotR * 2);
                        g.DrawEllipse(Pens.White, ptsNhap[i].X - dotR, ptsNhap[i].Y - dotR, dotR * 2, dotR * 2);
                        VeGiaTriTrenCot(g, duLieu[i].TongNhap.ToString(), fontSo, Brushes.DarkBlue, ptsNhap[i].X, ptsNhap[i].Y - 5);

                        g.FillEllipse(new SolidBrush(colXuat), ptsXuat[i].X - dotR, ptsXuat[i].Y - dotR, dotR * 2, dotR * 2);
                        g.DrawEllipse(Pens.White, ptsXuat[i].X - dotR, ptsXuat[i].Y - dotR, dotR * 2, dotR * 2);
                        VeGiaTriTrenCot(g, duLieu[i].TongXuat.ToString(), fontSo, Brushes.DarkRed, ptsXuat[i].X, ptsXuat[i].Y - 5);

                        g.FillEllipse(new SolidBrush(colTon), ptsTon[i].X - dotR, ptsTon[i].Y - dotR, dotR * 2, dotR * 2);
                        g.DrawEllipse(Pens.White, ptsTon[i].X - dotR, ptsTon[i].Y - dotR, dotR * 2, dotR * 2);
                        VeGiaTriTrenCot(g, duLieu[i].TonHienTai.ToString(), fontSo, Brushes.DarkGreen, ptsTon[i].X, ptsTon[i].Y - 5);
                    }
                }
            }
        }

        private static void VeCotGradient(Graphics g, float x, float y, float w, float h, Color colorDam, Color colorNhat)
        {
            if (h <= 0) return;
            RectangleF r = new RectangleF(x, y, w, h);
            using (LinearGradientBrush lgb = new LinearGradientBrush(r, colorDam, colorNhat, LinearGradientMode.Vertical))
            {
                g.FillRectangle(lgb, r);
            }
            using (Pen p = new Pen(ControlPaint.Dark(colorDam, 0.1f), 1))
            {
                g.DrawRectangle(p, x, y, w, h);
            }
        }

        private static void VeGiaTriTrenCot(Graphics g, string giaTri, Font f, Brush b, float centerX, float topY)
        {
            SizeF sz = g.MeasureString(giaTri, f);
            g.DrawString(giaTri, f, b, centerX - sz.Width / 2, topY - sz.Height - 1);
        }

        private static void VeNhanTrucX(Graphics g, string tenHang, string maHang, Font f, Brush b, float centerX, float topY, float maxWidth)
        {
            string hienThi = tenHang;
            if (hienThi.Length > 16)
            {
                hienThi = hienThi.Substring(0, 14) + "...";
            }
            hienThi += "\n(" + maHang + ")";

            RectangleF rect = new RectangleF(centerX - maxWidth / 2, topY, maxWidth, 45);
            StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near
            };
            g.DrawString(hienThi, f, b, rect, sf);
        }

        private static int TinhStepPhuHop(int maxVal, int count)
        {
            double roughStep = (double)maxVal / count;
            double pow10 = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));
            double normalized = roughStep / pow10;

            double clean;
            if (normalized <= 1.0) clean = 1.0;
            else if (normalized <= 2.0) clean = 2.0;
            else if (normalized <= 5.0) clean = 5.0;
            else clean = 10.0;

            return (int)Math.Max(1, clean * pow10);
        }

        public static bool LuuAnhBieuDo(List<ItemThongKe> duLieu, LoaiBieuDo loai, IWin32Window parent = null)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = "BieuDoNhapXuatTon_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                sfd.Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg";
                sfd.Title = "Lưu hình ảnh biểu đồ";

                if (sfd.ShowDialog(parent) != DialogResult.OK)
                {
                    return false;
                }

                try
                {
                    int w = 1200;
                    int h = 700;
                    using (Bitmap bmp = new Bitmap(w, h))
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        VeBieuDo(g, new Rectangle(0, 0, w, h), duLieu, loai);
                        ImageFormat fmt = sfd.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ? ImageFormat.Jpeg : ImageFormat.Png;
                        bmp.Save(sfd.FileName, fmt);
                    }

                    MessageBox.Show("Đã lưu hình ảnh biểu đồ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu ảnh biểu đồ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }
    }
}
