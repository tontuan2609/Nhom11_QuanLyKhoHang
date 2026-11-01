using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    public static class UiHelper
    {
        public static Label TaoNhan(Control cha, string noiDung, int x, int y)
        {
            Label nhan = new Label { Text = noiDung, Left = x, Top = y + 4, AutoSize = true };
            cha.Controls.Add(nhan);
            return nhan;
        }

        public static TextBox TaoTextBox(Control cha, int x, int y, int rong, int doDaiToiDa)
        {
            TextBox o = new TextBox { Left = x, Top = y, Width = rong, MaxLength = doDaiToiDa };
            cha.Controls.Add(o);
            return o;
        }

        public static NumericUpDown TaoSo(Control cha, int x, int y, int rong, decimal giaTriToiDa)
        {
            NumericUpDown o = new NumericUpDown
            {
                Left = x,
                Top = y,
                Width = rong,
                Minimum = 0,
                Maximum = giaTriToiDa,
                DecimalPlaces = 0,
                ThousandsSeparator = true,
                TextAlign = HorizontalAlignment.Right
            };
            cha.Controls.Add(o);
            return o;
        }

        public static Button TaoNut(Control cha, string noiDung, int x, int y, int rong)
        {
            Button nut = new Button { Text = noiDung, Left = x, Top = y, Width = rong, Height = 30 };
            cha.Controls.Add(nut);
            return nut;
        }

        public static DataGridView TaoLuoi()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window
            };
        }

        public static void GanSo(NumericUpDown o, object giaTri)
        {
            decimal v = Convert.ToDecimal(giaTri);
            o.Value = Math.Max(o.Minimum, Math.Min(o.Maximum, v));
        }

        public static void HienThongBao(string noiDung)
        {
            MessageBox.Show(noiDung, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void HienLoi(string noiDung)
        {
            MessageBox.Show(noiDung, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static bool XacNhan(string noiDung)
        {
            return MessageBox.Show(noiDung, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public static string MoTaLoiSql(SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601:
                    return "Dữ liệu bị trùng (mã hoặc tên đã tồn tại).";
                case 547:
                    return "Dữ liệu đang được sử dụng ở nơi khác hoặc vi phạm ràng buộc.";
                default:
                    return "Lỗi cơ sở dữ liệu: " + ex.Message;
            }
        }
    }
}
