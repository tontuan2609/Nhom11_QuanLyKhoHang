using System;
using System.Data;
using QuanLyKhoHang.Models;

namespace QuanLyKhoHang.Repositories
{
    public class TaiKhoanRepository
    {
        public ThongTinDangNhap XacThuc(string tenDangNhap, string matKhau)
        {
            DataTable dt = Database.GetData(
                "SELECT MaNguoiDung, TenDangNhap, HoTen, VaiTro FROM NguoiDung " +
                "WHERE TenDangNhap = @ten AND MatKhau = @mk COLLATE Latin1_General_CS_AS AND TrangThai = 1",
                Database.Param("@ten", tenDangNhap),
                Database.Param("@mk", matKhau));

            if (dt.Rows.Count == 0)
                return null;

            DataRow dong = dt.Rows[0];
            return new ThongTinDangNhap
            {
                MaNguoiDung = Convert.ToString(dong["MaNguoiDung"]),
                TenDangNhap = Convert.ToString(dong["TenDangNhap"]),
                HoTen = Convert.ToString(dong["HoTen"]),
                VaiTro = Convert.ToString(dong["VaiTro"])
            };
        }
    }
}
