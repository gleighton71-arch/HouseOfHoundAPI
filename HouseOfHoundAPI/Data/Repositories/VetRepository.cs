using HouseOfHoundAPI.Models.Vets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class VetRepository
{
    private readonly string _connectionString;

    public VetRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Vet> GetAll()
    {
        var vets = new List<Vet>();

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT Id, VetId, Name, Address, Phone, Email, ContactName, Url, IsActive, CreatedUtc
            FROM dbo.Vets
            ORDER BY Name;", conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    vets.Add(MapVet(reader));
                }
            }
        }

        return vets;
    }

    public Vet Get(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT Id, VetId, Name, Address, Phone, Email, ContactName, Url, IsActive, CreatedUtc
            FROM dbo.Vets
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? MapVet(reader) : null;
            }
        }
    }

    public int Create(Vet vet)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.Vets (VetId, Name, Address, Phone, Email, ContactName, Url, IsActive, CreatedUtc)
            OUTPUT INSERTED.Id
            VALUES (@VetId, @Name, @Address, @Phone, @Email, @ContactName, @Url, @IsActive, SYSUTCDATETIME());", conn))
        {
            AddParameters(cmd, vet);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public bool Update(int id, Vet vet)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.Vets
            SET VetId = @VetId,
                Name = @Name,
                Address = @Address,
                Phone = @Phone,
                Email = @Email,
                ContactName = @ContactName,
                Url = @Url,
                IsActive = @IsActive
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            AddParameters(cmd, vet);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool Delete(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("DELETE FROM dbo.Vets WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static void AddParameters(SqlCommand cmd, Vet vet)
    {
        cmd.Parameters.Add("@VetId", SqlDbType.NVarChar, 50).Value = vet.VetId;
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = vet.Name;
        cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object)vet.Address ?? DBNull.Value;
        cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value = (object)vet.Phone ?? DBNull.Value;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = (object)vet.Email ?? DBNull.Value;
        cmd.Parameters.Add("@ContactName", SqlDbType.NVarChar, 200).Value = (object)vet.ContactName ?? DBNull.Value;
        cmd.Parameters.Add("@Url", SqlDbType.NVarChar, 500).Value = (object)vet.Url ?? DBNull.Value;
        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = vet.IsActive;
    }

    private static Vet MapVet(SqlDataReader reader)
    {
        return new Vet
        {
            Id = (int)reader["Id"],
            VetId = reader["VetId"].ToString(),
            Name = reader["Name"].ToString(),
            Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
            Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
            Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
            ContactName = reader["ContactName"] == DBNull.Value ? null : reader["ContactName"].ToString(),
            Url = reader["Url"] == DBNull.Value ? null : reader["Url"].ToString(),
            IsActive = (bool)reader["IsActive"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }
}
