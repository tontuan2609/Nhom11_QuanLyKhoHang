namespace QuanLyKhoHang
{
    public static class PhienDangNhap
    {
        public const string VaiTroQuanLy = "Quản lý";
        public const string VaiTroNhanVienKho = "Nhân viên kho";

        public static string MaNguoiDung { get; private set; }
        public static string TenDangNhap { get; private set; }
        public static string HoTen { get; private set; }
        public static string VaiTro { get; private set; }

        public static bool DaDangNhap
        {
            get { return MaNguoiDung != null; }
        }

        public static bool LaQuanLy
        {
            get { return VaiTro == VaiTroQuanLy; }
        }

        public static void DangNhap(string maNguoiDung, string tenDangNhap, string hoTen, string vaiTro)
        {
            MaNguoiDung = maNguoiDung;
            TenDangNhap = tenDangNhap;
            HoTen = hoTen;
            VaiTro = vaiTro;
        }

        public static void DangXuat()
        {
            MaNguoiDung = null;
            TenDangNhap = null;
            HoTen = null;
            VaiTro = null;
        }
    }
}
