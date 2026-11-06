using System;
using System.Windows.Forms;
using QuanLyKhoHang.Views;
using QuanLyKhoHang.Presenters;

namespace QuanLyKhoHang
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FrmPhieuKho viewPhieu = new FrmPhieuKho();
            PhieuKhoPresenter presenterPhieu = new PhieuKhoPresenter(viewPhieu);
            Application.Run(viewPhieu);
        }
    }
}