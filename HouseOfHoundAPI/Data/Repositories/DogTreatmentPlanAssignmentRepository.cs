using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

public class DogTreatmentPlanAssignmentRepository
{
    private readonly string _connectionString;

    public DogTreatmentPlanAssignmentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<DogTreatmentPlanAssignment> GetHistory(int dogId)
    {
        var assignments = new List<DogTreatmentPlanAssignment>();

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT Id, DogId, SourceTreatmentPlanId, PlanName, PlanDescription, Interval, SessionCount,
                       CostPerSession, AssignedDateUtc, CompletedDateUtc, CreatedUtc
                FROM dbo.DogTreatmentPlanAssignments
                WHERE DogId = @DogId
                ORDER BY AssignedDateUtc DESC, Id DESC;", conn))
            {
                cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assignments.Add(MapAssignment(reader));
                    }
                }
            }

            foreach (var assignment in assignments)
            {
                assignment.Services = GetServices(conn, assignment.Id);
            }
        }

        return assignments;
    }

    public int Assign(int dogId, CreateDogTreatmentPlanAssignmentRequest request)
    {
        var assignedDate = request.AssignedDateUtc ?? DateTime.UtcNow;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int assignmentId;

                    using (var closeCmd = new SqlCommand(@"
                        UPDATE dbo.DogTreatmentPlanAssignments
                        SET CompletedDateUtc = @AssignedDateUtc
                        WHERE DogId = @DogId AND CompletedDateUtc IS NULL;", conn, tx))
                    {
                        closeCmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
                        closeCmd.Parameters.Add("@AssignedDateUtc", SqlDbType.DateTime2).Value = assignedDate;
                        closeCmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO dbo.DogTreatmentPlanAssignments
                            (DogId, SourceTreatmentPlanId, PlanName, PlanDescription, Interval, SessionCount, CostPerSession, AssignedDateUtc, CreatedUtc)
                        OUTPUT INSERTED.Id
                        SELECT
                            @DogId,
                            tp.TreatmentPlanId,
                            tp.PlanName,
                            tp.PlanDescription,
                            tp.Interval,
                            tp.SessionCount,
                            tp.CostPerSession,
                            @AssignedDateUtc,
                            SYSUTCDATETIME()
                        FROM dbo.TreatmentPlans tp
                        WHERE tp.TreatmentPlanId = @TreatmentPlanId;", conn, tx))
                    {
                        cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
                        cmd.Parameters.Add("@TreatmentPlanId", SqlDbType.Int).Value = request.TreatmentPlanId;
                        cmd.Parameters.Add("@AssignedDateUtc", SqlDbType.DateTime2).Value = assignedDate;
                        var result = cmd.ExecuteScalar();
                        if (result == null) throw new Exception("Treatment plan not found.");
                        assignmentId = (int)result;
                    }

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO dbo.DogTreatmentPlanAssignmentServices
                            (AssignmentId, SourceServiceId, Name, DurationMinutes, Cost, DisplayOrder)
                        SELECT
                            @AssignmentId,
                            s.ServiceId,
                            s.Name,
                            s.DurationMinutes,
                            s.Cost,
                            s.DisplayOrder
                        FROM dbo.TreatmentPlanServices s
                        WHERE s.TreatmentPlanId = @TreatmentPlanId
                        ORDER BY s.DisplayOrder;", conn, tx))
                    {
                        cmd.Parameters.Add("@AssignmentId", SqlDbType.Int).Value = assignmentId;
                        cmd.Parameters.Add("@TreatmentPlanId", SqlDbType.Int).Value = request.TreatmentPlanId;
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                    return assignmentId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }

    public bool Complete(int dogId, int assignmentId, DateTime? completedDateUtc)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
            UPDATE dbo.DogTreatmentPlanAssignments
            SET CompletedDateUtc = @CompletedDateUtc
            WHERE Id = @Id AND DogId = @DogId AND CompletedDateUtc IS NULL;", conn))
        {
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = assignmentId;
            cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = dogId;
            cmd.Parameters.Add("@CompletedDateUtc", SqlDbType.DateTime2).Value = completedDateUtc ?? DateTime.UtcNow;
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    private static List<DogTreatmentPlanAssignmentService> GetServices(SqlConnection conn, int assignmentId)
    {
        var services = new List<DogTreatmentPlanAssignmentService>();
        using (var cmd = new SqlCommand(@"
            SELECT Id, AssignmentId, SourceServiceId, Name, DurationMinutes, Cost, DisplayOrder
            FROM dbo.DogTreatmentPlanAssignmentServices
            WHERE AssignmentId = @AssignmentId
            ORDER BY DisplayOrder, Id;", conn))
        {
            cmd.Parameters.Add("@AssignmentId", SqlDbType.Int).Value = assignmentId;
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    services.Add(MapService(reader));
                }
            }
        }

        return services;
    }

    private static DogTreatmentPlanAssignment MapAssignment(SqlDataReader reader)
    {
        return new DogTreatmentPlanAssignment
        {
            Id = (int)reader["Id"],
            DogId = (int)reader["DogId"],
            SourceTreatmentPlanId = (int)reader["SourceTreatmentPlanId"],
            PlanName = reader["PlanName"].ToString(),
            PlanDescription = reader["PlanDescription"] == DBNull.Value ? null : reader["PlanDescription"].ToString(),
            Interval = reader["Interval"].ToString(),
            SessionCount = (int)reader["SessionCount"],
            CostPerSession = reader["CostPerSession"] == DBNull.Value ? (decimal?)null : (decimal)reader["CostPerSession"],
            AssignedDateUtc = (DateTime)reader["AssignedDateUtc"],
            CompletedDateUtc = reader["CompletedDateUtc"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["CompletedDateUtc"],
            CreatedUtc = (DateTime)reader["CreatedUtc"]
        };
    }

    private static DogTreatmentPlanAssignmentService MapService(SqlDataReader reader)
    {
        return new DogTreatmentPlanAssignmentService
        {
            Id = (int)reader["Id"],
            AssignmentId = (int)reader["AssignmentId"],
            SourceServiceId = reader["SourceServiceId"] == DBNull.Value ? (int?)null : (int)reader["SourceServiceId"],
            Name = reader["Name"].ToString(),
            DurationMinutes = reader["DurationMinutes"] == DBNull.Value ? (int?)null : (int)reader["DurationMinutes"],
            Cost = reader["Cost"] == DBNull.Value ? (decimal?)null : (decimal)reader["Cost"],
            DisplayOrder = (int)reader["DisplayOrder"]
        };
    }
}
