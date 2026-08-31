using HouseOfHoundAPI.Models.Dog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class DogService
    {

        public bool DogExists(int dogId)
        {
            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM Dogs WHERE DogId = @id";
              
                cmd.Parameters.AddWithValue("@id", dogId);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                return result > 0;

            }
           
        }

        public Dog GetDogById(int dogId)
        {
            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT DogId, Name, Breed, OwnerId FROM Dogs WHERE DogId = @id";
                cmd.Parameters.AddWithValue("@id", dogId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Dog
                        {
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Breed = reader.GetString(reader.GetOrdinal("Breed")),
                            OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
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