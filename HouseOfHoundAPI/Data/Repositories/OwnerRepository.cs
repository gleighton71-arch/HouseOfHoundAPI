using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Models.Owner;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class OwnerRepository
{
    private SqlConnection _connection;
    public OwnerRepository()
    {
        
    }

    public OwnerRepository(SqlConnection sqlConnection)
    {
        _connection = sqlConnection;    
    }

    public List<Owner> GetOwners()
    {
        using (var cmd = new SqlCommand("SELECT OwnerId, FullName, Email, Phone, Address FROM dbo.Owners;", _connection))
        {
            using (var reader = cmd.ExecuteReader())
            {
                var list = new List<Owner>();
                while (reader.Read())
                {
                    list.Add(new Owner
                    {
                        OwnerId = (int)reader["OwnerId"],
                        FullName = reader["FullName"].ToString(),
                        Email = reader["Email"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        Address = reader["Address"]?.ToString()
                    });
                }
                return list;
            }
        }
    }


    public int CreateOwner(CreateOwnerDto dto)
    {
        SqlTransaction transaction = _connection.BeginTransaction();

        int result = CreateOwner(_connection, transaction, dto);

        transaction.Commit();
        return result;
    }




    public int CreateOwnerWithDog(NewOwnerWithDogRequest request)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand("dbo.NewOwnerWithDog", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@OwnerFullName", request.OwnerFullName);
            cmd.Parameters.AddWithValue("@Email", request.Email);
            cmd.Parameters.AddWithValue("@Phone", request.Phone);
            cmd.Parameters.AddWithValue("@Address", request.Address);

            cmd.Parameters.AddWithValue("@DogName", request.DogName);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object)request.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Breed", (object)request.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object)request.Notes ?? DBNull.Value);

            return cmd.ExecuteNonQuery();
        }
    }

    public int CreateOwner(SqlConnection conn, SqlTransaction tx, CreateOwnerDto dto)
    {
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Owners (FullName, Email, Phone, Address)
OUTPUT INSERTED.OwnerId
VALUES (@FullName, @Email, @Phone, @Address);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@FullName", dto.FullName);
            cmd.Parameters.AddWithValue("@Email", (object)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)dto.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)dto.Address ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }
    }

    public bool OwnerExists(SqlConnection conn, SqlTransaction tx, int ownerId)
    {
        using (var cmd = new SqlCommand("SELECT 1 FROM dbo.Owners WHERE OwnerId = @OwnerId;", conn, tx))
        {
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            var result = cmd.ExecuteScalar();
            return result != null;
        }
    }

    public bool UpdateOwner(CreateOwnerDto dto)
    {
        // update the owner in the database using the provided dto
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.Owners
            SET FullName = @FullName, Email = @Email, Phone = @Phone, Address = @Address
            WHERE OwnerId = @OwnerId", _connection))
        {
            cmd.Parameters.AddWithValue("@OwnerId", dto.Id);
            cmd.Parameters.AddWithValue("@FullName", dto.FullName);
            cmd.Parameters.AddWithValue("@Email", (object)dto.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)dto.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object)dto.Address ?? DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}