using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class TherapistService
    {
        public bool TherapistExists(int Id)
        {
            using (var conn = Db.OpenConnection())
            {
               

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM Therapists WHERE TherapistId = @id";
                cmd.Parameters.AddWithValue("@id", Id);
                cmd.ExecuteNonQuery();
                
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                return result > 0;
            }
           
        }
    }
}