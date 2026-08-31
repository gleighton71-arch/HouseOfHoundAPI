using HouseOfHoundAPI.Models.Dog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class DogProviderAssignmentRepository
{
    private readonly string _connectionString;

    public DogProviderAssignmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<DogProviderAssignment> GetHistory(int dogId)
    {
        var history = new List<DogProviderAssignment>();

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(SelectSql + @"
            WHERE dpa.DogId = @DogId
            ORDER BY dpa.AssignedFromUtc DESC, dpa.Id DESC;", conn))
        {
            cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    history.Add(MapAssignment(reader));
                }
            }
        }

        return history;
    }

    public DogProviderAssignment GetCurrent(int dogId)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(SelectSql + @"
            WHERE dpa.DogId = @DogId AND dpa.AssignedToUtc IS NULL
            ORDER BY dpa.AssignedFromUtc DESC, dpa.Id DESC;", conn))
        {
            cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                return reader.Read() ? MapAssignment(reader) : null;
            }
        }
    }

    public int SetCurrent(int dogId, DogProviderAssignment assignment)
    {
        var assignedFrom = assignment.AssignedFromUtc == default(DateTime)
            ? DateTime.UtcNow
            : assignment.AssignedFromUtc;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    using (var closeCmd = new SqlCommand(@"
                        UPDATE dbo.DogProviderAssignments
                        SET AssignedToUtc = @AssignedFromUtc
                        WHERE DogId = @DogId AND AssignedToUtc IS NULL;", conn, tx))
                    {
                        closeCmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
                        closeCmd.Parameters.Add("@AssignedFromUtc", SqlDbType.DateTime2).Value = assignedFrom;
                        closeCmd.ExecuteNonQuery();
                    }

                    using (var insertCmd = new SqlCommand(@"
                        INSERT INTO dbo.DogProviderAssignments
                            (DogId, VetRecordId, SpecialistProviderRecordId, InsurerRecordId, InsurerPolicyRecordId, AssignedFromUtc, CreatedUtc)
                        OUTPUT INSERTED.Id
                        VALUES
                            (@DogId, @VetRecordId, @SpecialistProviderRecordId, @InsurerRecordId, @InsurerPolicyRecordId, @AssignedFromUtc, SYSUTCDATETIME());", conn, tx))
                    {
                        insertCmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
                        AddNullableInt(insertCmd, "@VetRecordId", assignment.VetRecordId);
                        AddNullableInt(insertCmd, "@SpecialistProviderRecordId", assignment.SpecialistProviderRecordId);
                        AddNullableInt(insertCmd, "@InsurerRecordId", assignment.InsurerRecordId);
                        AddNullableInt(insertCmd, "@InsurerPolicyRecordId", assignment.InsurerPolicyRecordId);
                        insertCmd.Parameters.Add("@AssignedFromUtc", SqlDbType.DateTime2).Value = assignedFrom;

                        var id = (int)insertCmd.ExecuteScalar();
                        tx.Commit();
                        return id;
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }

    private const string SelectSql = @"
        SELECT
            dpa.Id,
            dpa.DogId,
            dpa.VetRecordId,
            v.Name AS VetName,
            dpa.SpecialistProviderRecordId,
            sp.Name AS SpecialistProviderName,
            dpa.InsurerRecordId,
            i.Name AS InsurerName,
            dpa.InsurerPolicyRecordId,
            ip.Name AS PolicyName,
            dpa.AssignedFromUtc,
            dpa.AssignedToUtc,
            dpa.CreatedUtc
        FROM dbo.DogProviderAssignments dpa
        LEFT JOIN dbo.Vets v ON dpa.VetRecordId = v.Id
        LEFT JOIN dbo.SpecialistProviders sp ON dpa.SpecialistProviderRecordId = sp.Id
        LEFT JOIN dbo.Insurers i ON dpa.InsurerRecordId = i.Id
        LEFT JOIN dbo.InsurerPolicies ip ON dpa.InsurerPolicyRecordId = ip.Id";

    private static void AddNullableInt(SqlCommand cmd, string name, int? value)
    {
        cmd.Parameters.Add(name, SqlDbType.Int).Value = (object)value ?? DBNull.Value;
    }

    private static DogProviderAssignment MapAssignment(SqlDataReader reader)
    {
        return new DogProviderAssignment
        {
            Id = (int)reader["Id"],
            DogId = (int)reader["DogId"],
            VetRecordId = reader["VetRecordId"] == DBNull.Value ? (int?)null : (int)reader["VetRecordId"],
            VetName = reader["VetName"] == DBNull.Value ? null : reader["VetName"].ToString(),
            SpecialistProviderRecordId = reader["SpecialistProviderRecordId"] == DBNull.Value ? (int?)null : (int)reader["SpecialistProviderRecordId"],
            SpecialistProviderName = reader["SpecialistProviderName"] == DBNull.Value ? null : reader["SpecialistProviderName"].ToString(),
            InsurerRecordId = reader["InsurerRecordId"] == DBNull.Value ? (int?)null : (int)reader["InsurerRecordId"],
            InsurerName = reader["InsurerName"] == DBNull.Value ? null : reader["InsurerName"].ToString(),
            InsurerPolicyRecordId = reader["InsurerPolicyRecordId"] == DBNull.Value ? (int?)null : (int)reader["InsurerPolicyRecordId"],
            PolicyName = reader["PolicyName"] == DBNull.Value ? null : reader["PolicyName"].ToString(),
            AssignedFromUtc = (DateTime)reader["AssignedFromUtc"],
            AssignedToUtc = reader["AssignedToUtc"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["AssignedToUtc"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }
}
