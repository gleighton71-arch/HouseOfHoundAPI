using HouseOfHoundAPI.Data;
using HouseOfHoundAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class TherapistRepository : ITherapistRepository
{
    private readonly string _connectionString;

    public TherapistRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<TherapistDto> GetAll()
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT TherapistId,
                   Name,
                   RegistrationNumber,
                   Email,
                   Phone,
                   IsActive,
                   IdentityUserId,
                   CreatedUtc
            FROM dbo.Therapists
            ORDER BY Name;", conn))
        {
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                var list = new List<TherapistDto>();

                while (reader.Read())
                {
                    list.Add(new TherapistDto
                    {
                        TherapistId = (int)reader["TherapistId"],
                        Name = reader["Name"].ToString(),
                        RegistrationNumber = reader["RegistrationNumber"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Phone = reader["Phone"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        IdentityUserId = reader["IdentityUserId"]?.ToString(),
                        CreatedUtc = (DateTime)reader["CreatedUtc"]
                    });
                }

                return list;
            }
        }
    }

    public TherapistDto Get(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            SELECT TherapistId,
                   Name,
                   RegistrationNumber,
                   Email,
                   Phone,
                   IsActive,
                   IdentityUserId,
                   CreatedUtc
            FROM dbo.Therapists
            WHERE TherapistId = @TherapistId;", conn))
        {
            cmd.Parameters.Add("@TherapistId", SqlDbType.Int).Value = id;

            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    return null;

                return new TherapistDto
                {
                    TherapistId = (int)reader["TherapistId"],
                    Name = reader["Name"].ToString(),
                    RegistrationNumber = reader["RegistrationNumber"]?.ToString(),
                    Email = reader["Email"]?.ToString(),
                    Phone = reader["Phone"]?.ToString(),
                    IsActive = (bool)reader["IsActive"],
                    IdentityUserId = reader["IdentityUserId"]?.ToString(),
                    CreatedUtc = (DateTime)reader["CreatedUtc"]
                };
            }
        }
    }

    public int Create(CreateTherapistDto dto)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            INSERT INTO dbo.Therapists
            (
                Name,
                RegistrationNumber,
                Email,
                Phone,
                IsActive,
                IdentityUserId,
                CreatedUtc
            )
            OUTPUT INSERTED.TherapistId
            VALUES
            (
                @Name,
                @RegistrationNumber,
                @Email,
                @Phone,
                @IsActive,
                @IdentityUserId,
                SYSUTCDATETIME()
            );", conn))
        {
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = dto.Name;
            cmd.Parameters.Add("@RegistrationNumber", SqlDbType.NVarChar, 50)
                .Value = (object)dto.RegistrationNumber ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200)
                .Value = (object)dto.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50)
                .Value = (object)dto.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = dto.IsActive;
            cmd.Parameters.Add("@IdentityUserId", SqlDbType.NVarChar, 128)
                .Value = (object)dto.IdentityUserId ?? DBNull.Value;

            conn.Open();

            return (int)cmd.ExecuteScalar();
        }
    }

    public void Update(int id, UpdateTherapistDto dto)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.Therapists
            SET Name = @Name,
                RegistrationNumber = @RegistrationNumber,
                Email = @Email,
                Phone = @Phone,
                IsActive = @IsActive,
                IdentityUserId = @IdentityUserId
            WHERE TherapistId = @TherapistId;", conn))
        {
            cmd.Parameters.Add("@TherapistId", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 150).Value = dto.Name;
            cmd.Parameters.Add("@RegistrationNumber", SqlDbType.NVarChar, 50)
                .Value = (object)dto.RegistrationNumber ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200)
                .Value = (object)dto.Email ?? DBNull.Value;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50)
                .Value = (object)dto.Phone ?? DBNull.Value;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = dto.IsActive;
            cmd.Parameters.Add("@IdentityUserId", SqlDbType.NVarChar, 128)
                .Value = (object)dto.IdentityUserId ?? DBNull.Value;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void Delete(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            DELETE FROM dbo.Therapists
            WHERE TherapistId = @TherapistId;", conn))
        {
            cmd.Parameters.Add("@TherapistId", SqlDbType.Int).Value = id;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}