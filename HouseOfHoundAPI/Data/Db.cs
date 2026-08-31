using System.Configuration;
using System.Data.SqlClient;

public static class Db
{
    public static SqlConnection OpenConnection()
    {
        var cs = ConfigurationManager.ConnectionStrings["HoH"].ConnectionString;
        var conn = new SqlConnection(cs);
        conn.Open();
        return conn;
    }

    public static string GetConnectionString()
    {
        return ConfigurationManager.ConnectionStrings["HoH"].ConnectionString;
    }
}