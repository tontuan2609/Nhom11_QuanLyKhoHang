using System;
using System.Windows.Forms;

namespace QuanLyKhoHang
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true)
            {
                using (FrmLogin dangNhap = new FrmLogin())
                {
                    if (dangNhap.ShowDialog() != DialogResult.OK)
                    {
                        break;
                    }
                }

                bool dangXuat;
                using (FrmMain chinh = new FrmMain())
                {
                    Application.Run(chinh);
                    dangXuat = chinh.DangXuat;
                }

                PhienDangNhap.DangXuat();
                if (!dangXuat)
                {
                    break;
                }
            }
        }
    }
}
