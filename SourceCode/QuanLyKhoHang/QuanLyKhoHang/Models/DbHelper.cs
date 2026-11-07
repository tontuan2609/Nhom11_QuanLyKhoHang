using System.Data.SqlClient;

namespace QuanLyKhoHang.Models
{
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get { return Database.ChuoiKetNoi; }
            set { }
        }

        public static SqlConnection GetConnection()
        {
            return Database.TaoKetNoi();
        }
    }
}