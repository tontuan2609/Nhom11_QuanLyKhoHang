using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace QuanLyKhoHang
{
    public static class Database
    {
        private const string ChuoiKetNoiMacDinh = @"Server=.\SQLEXPRESS;Database=QuanLyKhoHangDB;Integrated Security=True;TrustServerCertificate=True";

        public static string ChuoiKetNoi
        {
            get
            {
                ConnectionStringSettings cauHinh = ConfigurationManager.ConnectionStrings["QuanLyKhoHangDB"];
                return cauHinh != null ? cauHinh.ConnectionString : ChuoiKetNoiMacDinh;
            }
        }

        public static SqlConnection TaoKetNoi()
        {
            return new SqlConnection(ChuoiKetNoi);
        }

        public static bool KiemTraKetNoi(out string loi)
        {
            try
            {
                using (SqlConnection conn = TaoKetNoi())
                {
                    conn.Open();
                }
                loi = null;
                return true;
            }
            catch (Exception ex)
            {
                loi = ex.Message;
                return false;
            }
        }

        public static SqlParameter Param(string ten, object giaTri)
        {
            return new SqlParameter(ten, giaTri ?? DBNull.Value);
        }

        public static DataTable GetData(string sql, params SqlParameter[] thamSo)
        {
            return LayDuLieu(sql, CommandType.Text, thamSo);
        }

        public static DataTable GetDataProc(string tenProc, params SqlParameter[] thamSo)
        {
            return LayDuLieu(tenProc, CommandType.StoredProcedure, thamSo);
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] thamSo)
        {
            using (SqlConnection conn = TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (thamSo != null)
                {
                    cmd.Parameters.AddRange(thamSo);
                }
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] thamSo)
        {
            using (SqlConnection conn = TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (thamSo != null)
                {
                    cmd.Parameters.AddRange(thamSo);
                }
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static void ChayTrongTransaction(Action<SqlConnection, SqlTransaction> congViec)
        {
            using (SqlConnection conn = TaoKetNoi())
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        congViec(conn, tran);
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private static DataTable LayDuLieu(string lenh, CommandType loai, SqlParameter[] thamSo)
        {
            using (SqlConnection conn = TaoKetNoi())
            using (SqlCommand cmd = new SqlCommand(lenh, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = loai;
                if (thamSo != null)
                {
                    cmd.Parameters.AddRange(thamSo);
                }
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }
    }
}
