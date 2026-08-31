using HouseOfHoundAPI.Models.AppointmentTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class AppointmentTypeRepository
{
    private readonly string _connectionString;

    public AppointmentTypeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<AppointmentType> GetAll()
    {
        var types = new List<AppointmentType>();

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
SELECT Id, Code, Description, Cost, DurationMinutes, CreatedUtc
FROM dbo.AppointmentTypes
ORDER BY Code;", conn))
        {
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    types.Add(Map(reader));
                }
            }
        }

        return types;
    }

    public AppointmentType Get(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
SELECT Id, Code, Description, Cost, DurationMinutes, CreatedUtc
FROM dbo.AppointmentTypes
WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();

            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? Map(reader) : null;
            }
        }
    }

    public int Create(AppointmentType type)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.AppointmentTypes (Code, Description, Cost, DurationMinutes, CreatedUtc)
OUTPUT INSERTED.Id
VALUES (@Code, @Description, @Cost, @DurationMinutes, SYSUTCDATETIME());", conn))
        {
            AddParameters(cmd, type);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public bool Update(int id, AppointmentType type)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
UPDATE dbo.AppointmentTypes
SET Code = @Code,
    Description = @Description,
    Cost = @Cost,
    DurationMinutes = @DurationMinutes
WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            AddParameters(cmd, type);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public bool Delete(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand("DELETE FROM dbo.AppointmentTypes WHERE Id = @Id;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static void AddParameters(SqlCommand cmd, AppointmentType type)
    {
        cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = type.Code;
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 250).Value = type.Description;
        cmd.Parameters.Add("@Cost", SqlDbType.Decimal).Value = type.Cost;
        cmd.Parameters["@Cost"].Precision = 10;
        cmd.Parameters["@Cost"].Scale = 2;
        cmd.Parameters.Add("@DurationMinutes", SqlDbType.Int).Value = type.DurationMinutes;
    }

    private static AppointmentType Map(SqlDataReader reader)
    {
        return new AppointmentType
        {
            Id = (int)reader["Id"],
            Code = reader["Code"].ToString(),
            Description = reader["Description"].ToString(),
            Cost = (decimal)reader["Cost"],
            DurationMinutes = (int)reader["DurationMinutes"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }
}
