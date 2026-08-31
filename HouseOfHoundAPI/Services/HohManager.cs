using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public static class HohManager
    {
        public static string GetConnectionString()
        {
            // In a real application, you would likely want to cache this value
            // rather than reading it from configuration every time.
            return System.Configuration.ConfigurationManager.ConnectionStrings["HoH"].ConnectionString;
        }

        public static SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }
    }
}