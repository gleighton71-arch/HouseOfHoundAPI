using HouseOfHoundAPI.Models.SpecialistProviders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class SpecialistProviderRepository
{
    private readonly string _connectionString;

    public SpecialistProviderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<SpecialistProvider> GetAll()
    {
        var providers = new List<SpecialistProvider>();

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT Id, SpecialistId, Name, IsActive, CreatedUtc
            FROM dbo.SpecialistProviders
            ORDER BY Name;", conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    providers.Add(MapProvider(reader));
                }
            }
        }

        return providers;
    }

    public SpecialistProvider Get(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT Id, SpecialistId, Name, IsActive, CreatedUtc
            FROM dbo.SpecialistProviders
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? MapProvider(reader) : null;
            }
        }
    }

    public int Create(SpecialistProvider provider)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.SpecialistProviders (SpecialistId, Name, IsActive, CreatedUtc)
            OUTPUT INSERTED.Id
            VALUES (@SpecialistId, @Name, @IsActive, SYSUTCDATETIME());", conn))
        {
            AddParameters(cmd, provider);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public bool Update(int id, SpecialistProvider provider)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.SpecialistProviders
            SET SpecialistId = @SpecialistId,
                Name = @Name,
                IsActive = @IsActive
            WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            AddParameters(cmd, provider);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool Delete(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("DELETE FROM dbo.SpecialistProviders WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static void AddParameters(SqlCommand cmd, SpecialistProvider provider)
    {
        cmd.Parameters.Add("@SpecialistId", SqlDbType.NVarChar, 50).Value = provider.SpecialistId;
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = provider.Name;
        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = provider.IsActive;
    }

    private static SpecialistProvider MapProvider(SqlDataReader reader)
    {
        return new SpecialistProvider
        {
            Id = (int)reader["Id"],
            SpecialistId = reader["SpecialistId"].ToString(),
            Name = reader["Name"].ToString(),
            IsActive = (bool)reader["IsActive"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }
}
