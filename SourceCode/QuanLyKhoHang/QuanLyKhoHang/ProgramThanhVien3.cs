using System;
using System.Windows.Forms;
using QuanLyKhoHang.Views;

namespace QuanLyKhoHang
{
    /// <summary>
    /// Điểm khởi chạy riêng biệt để test trực tiếp giao diện Form Thành Viên 3 (chuẩn kiến trúc MVP)
    /// (Tra cứu lịch sử nhập xuất, Tính tồn kho Stored Proc, Xuất CSV, Biểu đồ GDI+, In phiếu kho PrintDocument).
    /// </summary>
    internal static class ProgramThanhVien3
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmThongKeLichSu());
        }
    }
}
