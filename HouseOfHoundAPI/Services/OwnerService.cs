using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Models.Owner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class OwnerService
    {
        public bool OwnerExists(int ownerId)
        {
            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM Owners WHERE OwnerId = @OwnerId";
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                return result > 0;
            }
        }

        public Owner GetOwnerByDogId(int dogId)
        {
            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT o.OwnerId, o.FullName, o.Email, o.Phone, o.Address 
                                    FROM Owners o
                                    INNER JOIN Dogs d ON o.OwnerId = d.OwnerId
                                    WHERE d.DogId = @DogId";
                cmd.Parameters.AddWithValue("@DogId", dogId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Owner
                        {
                            OwnerId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Email = reader.GetString(2),
                            Phone = reader.GetString(3),
                            Address = reader.GetString(4)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

     

        public Owner GetOwner(int ownerId)
        {
            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT OwnerId, FullName, Email, Phone, Address FROM Owners WHERE OwnerId = @OwnerId";
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Owner
                        {
                            OwnerId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Email = reader.GetString(2),
                            Phone = reader.GetString(3),
                            Address = reader.GetString(4)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

    }

    
}