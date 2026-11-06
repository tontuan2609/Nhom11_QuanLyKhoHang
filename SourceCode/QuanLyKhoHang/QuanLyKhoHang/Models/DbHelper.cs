using System.Data.SqlClient;

namespace QuanLyKhoHang.Models
{
    public static class DbHelper
    {
        public static string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=QuanLyKhoHangDB;Integrated Security=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}