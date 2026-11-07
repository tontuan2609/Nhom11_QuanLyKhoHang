
using System.Data;
using System;

namespace QuanLyKhoHang.Repositories
{
    public class LoaiHangRepository
    {
        public DataTable LayDanhSach(string tuKhoa)
        {
            string sql =
                "SELECT l.MaLoai, l.TenLoai, l.MoTa, " +
                "COUNT(h.MaHang) AS SoHangHoa " +
                "FROM LoaiHang l " +
                "LEFT JOIN HangHoa h ON h.MaLoai = l.MaLoai " +
                "WHERE l.MaLoai LIKE @k OR l.TenLoai LIKE @k " +
                "GROUP BY l.MaLoai, l.TenLoai, l.MoTa " +
                "ORDER BY l.MaLoai";

            return Database.GetData(
                sql,
                Database.Param("@k", "%" + tuKhoa.Trim() + "%")
            );
        }

        public bool KiemTraMaTonTai(string ma)
        {
            int soLuong = Convert.ToInt32(Database.ExecuteScalar(
                "SELECT COUNT(*) FROM LoaiHang WHERE MaLoai = @ma",
                Database.Param("@ma", ma)));

            return soLuong > 0;
        }

        public int Them(string ma, string ten, string moTa)
        {
            return Database.ExecuteNonQuery(
                "INSERT INTO LoaiHang(MaLoai, TenLoai, MoTa) " +
                "VALUES (@ma, @ten, @moTa)",
                Database.Param("@ma", ma),
                Database.Param("@ten", ten),
                Database.Param("@moTa",
                    string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim()));
        }

        public int Sua(string ma, string ten, string moTa)
        {
            return Database.ExecuteNonQuery(
                "UPDATE LoaiHang SET TenLoai = @ten, MoTa = @moTa " +
                "WHERE MaLoai = @ma",
                Database.Param("@ma", ma),
                Database.Param("@ten", ten),
                Database.Param("@moTa",
                    string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim()));
        }

        public int DemHangHoa(string ma)
        {
            return Convert.ToInt32(Database.ExecuteScalar(
                "SELECT COUNT(*) FROM HangHoa WHERE MaLoai = @ma",
                Database.Param("@ma", ma)));
        }

        public int Xoa(string ma)
        {
            return Database.ExecuteNonQuery(
                "DELETE FROM LoaiHang WHERE MaLoai = @ma",
                Database.Param("@ma", ma));
        }

    }

}
