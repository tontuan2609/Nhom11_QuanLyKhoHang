using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace QuanLyKhoHang.Services
{
    /// <summary>
    /// Service xử lý xuất file CSV chuẩn UTF-8 có BOM
    /// Mở trực tiếp bằng Microsoft Excel không bao giờ bị lỗi font tiếng Việt có dấu
    /// </summary>
    public static class CsvHelperCustom
    {
        public static bool XuatDataTableRaCsv(DataTable dt, string tieuDeMacDinh, IWin32Window parent = null)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                sfd.FileName = tieuDeMacDinh + "_" + timestamp + ".csv";
                sfd.Filter = "CSV UTF-8 (Comma delimited) (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.Title = "Lưu file CSV";

                if (sfd.ShowDialog(parent) != DialogResult.OK)
                {
                    return false;
                }

                try
                {
                    StringBuilder sb = new StringBuilder();

                    // Tiêu đề cột
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sb.Append(ChuanHoaO(dt.Columns[i].ColumnName));
                        if (i < dt.Columns.Count - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    sb.AppendLine();

                    // Dòng dữ liệu
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            object val = row[i];
                            string chuoiGiaTri = "";

                            if (val != null && val != DBNull.Value)
                            {
                                if (val is DateTime)
                                {
                                    chuoiGiaTri = ((DateTime)val).ToString("dd/MM/yyyy HH:mm");
                                }
                                else if (val is decimal || val is double || val is float)
                                {
                                    chuoiGiaTri = string.Format("{0:0.##}", val);
                                }
                                else
                                {
                                    chuoiGiaTri = val.ToString();
                                }
                            }

                            sb.Append(ChuanHoaO(chuoiGiaTri));
                            if (i < dt.Columns.Count - 1)
                            {
                                sb.Append(",");
                            }
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));

                    DialogResult dr = MessageBox.Show(
                        "Xuất file CSV thành công!\n\nĐường dẫn: " + sfd.FileName + "\n\nBạn có muốn mở file ngay bây giờ không?",
                        "Xuất file thành công",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public static bool XuatDataGridViewRaCsv(DataGridView dgv, string tieuDeMacDinh, IWin32Window parent = null)
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu trên bảng để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                sfd.FileName = tieuDeMacDinh + "_" + timestamp + ".csv";
                sfd.Filter = "CSV UTF-8 (Comma delimited) (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.Title = "Lưu file CSV";

                if (sfd.ShowDialog(parent) != DialogResult.OK)
                {
                    return false;
                }

                try
                {
                    StringBuilder sb = new StringBuilder();

                    bool daGhiCotDau = false;
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!col.Visible) continue;
                        if (daGhiCotDau) sb.Append(",");
                        sb.Append(ChuanHoaO(col.HeaderText));
                        daGhiCotDau = true;
                    }
                    sb.AppendLine();

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;
                        bool daGhiO = false;
                        foreach (DataGridViewColumn col in dgv.Columns)
                        {
                            if (!col.Visible) continue;
                            if (daGhiO) sb.Append(",");
                            object val = row.Cells[col.Index].Value;
                            sb.Append(ChuanHoaO(val != null ? val.ToString() : ""));
                            daGhiO = true;
                        }
                        sb.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));

                    DialogResult dr = MessageBox.Show(
                        "Xuất file CSV thành công!\n\nĐường dẫn: " + sfd.FileName + "\n\nBạn có muốn mở file ngay bây giờ không?",
                        "Xuất file thành công",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (dr == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file CSV: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        private static string ChuanHoaO(string giaTri)
        {
            if (string.IsNullOrEmpty(giaTri))
            {
                return "\"\"";
            }

            if (giaTri.Contains(",") || giaTri.Contains("\"") || giaTri.Contains("\n") || giaTri.Contains("\r"))
            {
                return "\"" + giaTri.Replace("\"", "\"\"") + "\"";
            }

            return "\"" + giaTri + "\"";
        }
    }
}
