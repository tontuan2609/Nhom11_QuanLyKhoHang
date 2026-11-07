using System;
using System.Data;
using QuanLyKhoHang.Models;

namespace QuanLyKhoHang.Repositories
{
    public class HangHoaRepository
    {
        public DataTable LayDanhSachLoai()
        {
            return Database.GetData("SELECT MaLoai, TenLoai FROM LoaiHang ORDER BY TenLoai");
        }

        public DataTable LayDanhSach(string tuKhoa, string maLoai)
        {
            return Database.GetDataProc("sp_LayDanhSachHangHoa",
                Database.Param("@TuKhoa", (tuKhoa ?? "").Trim()),
                Database.Param("@MaLoai", maLoai ?? ""));
        }

        public bool KiemTraMaTonTai(string ma)
        {
            return Convert.ToInt32(Database.ExecuteScalar(
                "SELECT COUNT(*) FROM HangHoa WHERE MaHang = @ma", Database.Param("@ma", ma))) > 0;
        }

        public void Them(HangHoa h)
        {
            Database.ExecuteNonQuery(
                "INSERT INTO HangHoa(MaHang, TenHang, MaLoai, DonViTinh, DonGiaNhap, SoLuongTon, MucTonToiThieu, HinhAnh) " +
                "VALUES (@ma, @ten, @loai, @dvt, @gia, @ton, @toiThieu, @anh)",
                Database.Param("@ma", h.MaHang), Database.Param("@ten", h.TenHang),
                Database.Param("@loai", h.MaLoai), Database.Param("@dvt", h.DonViTinh),
                Database.Param("@gia", h.DonGiaNhap), Database.Param("@ton", h.SoLuongTon),
                Database.Param("@toiThieu", h.MucTonToiThieu), Database.Param("@anh", h.HinhAnh));
        }

        public int Sua(HangHoa h)
        {
            return Database.ExecuteNonQuery(
                "UPDATE HangHoa SET TenHang = @ten, MaLoai = @loai, DonViTinh = @dvt, DonGiaNhap = @gia, " +
                "MucTonToiThieu = @toiThieu, HinhAnh = @anh WHERE MaHang = @ma",
                Database.Param("@ten", h.TenHang), Database.Param("@loai", h.MaLoai),
                Database.Param("@dvt", h.DonViTinh), Database.Param("@gia", h.DonGiaNhap),
                Database.Param("@toiThieu", h.MucTonToiThieu), Database.Param("@anh", h.HinhAnh),
                Database.Param("@ma", h.MaHang));
        }

        public int DemChiTietPhieu(string ma)
        {
            return Convert.ToInt32(Database.ExecuteScalar(
                "SELECT COUNT(*) FROM ChiTietPhieu WHERE MaHang = @ma", Database.Param("@ma", ma)));
        }

        public int Xoa(string ma)
        {
            return Database.ExecuteNonQuery("DELETE FROM HangHoa WHERE MaHang = @ma", Database.Param("@ma", ma));
        }
    }
}
