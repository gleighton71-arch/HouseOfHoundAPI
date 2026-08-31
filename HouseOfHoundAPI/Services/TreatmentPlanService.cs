using HouseOfHoundAPI.Models.Treatment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{

    public class TreatmentPlanService
    {
        public List<TreatmentPlanDto> GetTreatmentPlans()
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            SELECT 
                tp.TreatmentPlanId,
                tp.PlanName,
                tp.PlanDescription,
                tp.Interval,
                tp.SessionCount,

                s.ServiceId,
                s.Name AS ServiceName,
                s.DurationMinutes,
                s.Cost AS ServiceCost,

                a.ActionId,
                a.Description,
                a.Duration

            FROM dbo.TreatmentPlans tp
            LEFT JOIN dbo.TreatmentPlanServices s 
                ON s.TreatmentPlanId = tp.TreatmentPlanId
            LEFT JOIN dbo.TreatmentPlanActions a 
                ON a.ServiceId = s.ServiceId

            WHERE tp.Active = 1

            ORDER BY 
                tp.TreatmentPlanId,
                s.DisplayOrder,
                a.DisplayOrder
        ", conn))
            {
                var planLookup = new Dictionary<int, TreatmentPlanDto>();

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var planId = rdr.GetInt32(rdr.GetOrdinal("TreatmentPlanId"));

                        // 🔹 PLAN
                        if (!planLookup.TryGetValue(planId, out var plan))
                        {
                            plan = new TreatmentPlanDto
                            {
                                Id = planId,
                                PlanName = rdr.GetString(rdr.GetOrdinal("PlanName")),
                                PlanDescription = rdr["PlanDescription"] as string,
                                Interval = rdr.GetString(rdr.GetOrdinal("Interval")),
                                SessionCount = rdr.GetInt32(rdr.GetOrdinal("SessionCount"))
                            };

                            planLookup.Add(planId, plan);
                        }

                        // 🔹 SERVICE
                        if (rdr["ServiceId"] != DBNull.Value)
                        {
                            var serviceId = (int)rdr["ServiceId"];

                            var service = plan.Services
                                .FirstOrDefault(s => s.Id == serviceId);

                            if (service == null)
                            {
                                service = new TreatmentServiceDto
                                {
                                    Id = serviceId,
                                    Name = rdr["ServiceName"] as string,
                                    DurationMinutes = rdr["DurationMinutes"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["DurationMinutes"]),
                                    Cost = rdr["ServiceCost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(rdr["ServiceCost"])
                                };

                                plan.Services.Add(service);
                            }

                            // 🔹 ACTION
                            if (rdr["ActionId"] != DBNull.Value)
                            {
                                service.Actions.Add(new TreatmentActionDto
                                {
                                    Description = rdr["Description"] as string,
                                    Duration = rdr["Duration"] as string
                                });
                            }
                        }
                    }
                }

                var plans = planLookup.Values.ToList();
                foreach (var plan in plans)
                {
                    plan.CostPerSession = plan.Services.Sum(service => service.Cost ?? 0);
                }

                return plans;
            }
        }

        public void UpdateTreatmentPlan(int planId, TreatmentPlanDto model)
        {
            using (var conn = Db.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    // 🔹 UPDATE PLAN
                    using (var cmd = new SqlCommand(@"
                UPDATE dbo.TreatmentPlans
                SET
                    PlanName = @PlanName,
                    PlanDescription = @PlanDescription,
                    Interval = @Interval,
                    SessionCount = @SessionCount,
                    CostPerSession = @CostPerSession
                WHERE TreatmentPlanId = @PlanId", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PlanId", planId);
                        cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
                        cmd.Parameters.AddWithValue("@PlanDescription", (object)model.PlanDescription ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Interval", model.Interval);
                        cmd.Parameters.AddWithValue("@SessionCount", model.SessionCount);
                        cmd.Parameters.AddWithValue("@CostPerSession", CalculateCostPerSession(model));

                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 DELETE EXISTING CHILDREN (CASCADE will handle actions)
                    using (var cmd = new SqlCommand(@"
                DELETE FROM dbo.TreatmentPlanServices
                WHERE TreatmentPlanId = @PlanId", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PlanId", planId);
                        cmd.ExecuteNonQuery();
                    }

                    // 🔹 REINSERT SERVICES + ACTIONS
                    int serviceOrder = 1;

                    foreach (var service in model.Services ?? new List<TreatmentServiceDto>())
                    {
                        int serviceId;

                        using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.TreatmentPlanServices
                    (
                        TreatmentPlanId,
                        Name,
                        DurationMinutes,
                        Cost,
                        DisplayOrder
                    )
                    OUTPUT INSERTED.ServiceId
                    VALUES
                    (
                        @PlanId,
                        @Name,
                        @DurationMinutes,
                        @Cost,
                        @DisplayOrder
                    )", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@PlanId", planId);
                            cmd.Parameters.AddWithValue("@Name", service.Name??"");
                            cmd.Parameters.AddWithValue("@DurationMinutes", service.DurationMinutes.HasValue ? (object)service.DurationMinutes.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cost", service.Cost.HasValue ? (object)service.Cost.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@DisplayOrder", serviceOrder++);

                            serviceId = (int)cmd.ExecuteScalar();
                        }

                        int actionOrder = 1;

                        foreach (var action in service.Actions ?? new List<TreatmentActionDto>())
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO dbo.TreatmentPlanActions
                        (
                            ServiceId,
                            Description,
                            Duration,
                            DisplayOrder
                        )
                        VALUES
                        (
                            @ServiceId,
                            @Description,
                            @Duration,
                            @DisplayOrder
                        )", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                                cmd.Parameters.AddWithValue("@Description", action.Description);
                                cmd.Parameters.AddWithValue("@Duration", (object)action.Duration ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@DisplayOrder", actionOrder++);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public int CreateTreatmentPlan(TreatmentPlanDto model)
        {
            using (var conn = Db.OpenConnection())
            using (var tran = conn.BeginTransaction())
            {
                try
                {
                    int planId;

                    // 🔹 INSERT PLAN
                    using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.TreatmentPlans
                (
                    PlanName,
                    PlanDescription,
                    Interval,
                    SessionCount,
                    CostPerSession
                )
                OUTPUT INSERTED.TreatmentPlanId
                VALUES
                (
                    @PlanName,
                    @PlanDescription,
                    @Interval,
                    @SessionCount,
                    @CostPerSession
                )", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PlanName", model.PlanName);
                        cmd.Parameters.AddWithValue("@PlanDescription", (object)model.PlanDescription ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Interval", model.Interval);
                        cmd.Parameters.AddWithValue("@SessionCount", model.SessionCount);
                        cmd.Parameters.AddWithValue("@CostPerSession", CalculateCostPerSession(model));

                        planId = (int)cmd.ExecuteScalar();
                    }

                    // 🔹 INSERT SERVICES + ACTIONS
                    int serviceOrder = 1;

                    foreach (var service in model.Services ?? new List<TreatmentServiceDto>())
                    {
                        int serviceId;

                        // INSERT SERVICE
                        using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.TreatmentPlanServices
                    (
                        TreatmentPlanId,
                        Name,
                        DurationMinutes,
                        Cost,
                        DisplayOrder
                    )
                    OUTPUT INSERTED.ServiceId
                    VALUES
                    (
                        @PlanId,
                        @Name,
                        @DurationMinutes,
                        @Cost,
                        @DisplayOrder
                    )", conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@PlanId", planId);
                            cmd.Parameters.AddWithValue("@Name", service.Name??"");
                            cmd.Parameters.AddWithValue("@DurationMinutes", service.DurationMinutes.HasValue ? (object)service.DurationMinutes.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cost", service.Cost.HasValue ? (object)service.Cost.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@DisplayOrder", serviceOrder++);

                            serviceId = (int)cmd.ExecuteScalar();
                        }

                        // INSERT ACTIONS
                        int actionOrder = 1;

                        foreach (var action in service.Actions ?? new List<TreatmentActionDto>())
                        {
                            using (var cmd = new SqlCommand(@"
                        INSERT INTO dbo.TreatmentPlanActions
                        (
                            ServiceId,
                            Description,
                            Duration,
                            DisplayOrder
                        )
                        VALUES
                        (
                            @ServiceId,
                            @Description,
                            @Duration,
                            @DisplayOrder
                        )", conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@ServiceId", serviceId);
                                cmd.Parameters.AddWithValue("@Description", action.Description);
                                cmd.Parameters.AddWithValue("@Duration", (object)action.Duration ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@DisplayOrder", actionOrder++);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    tran.Commit();
                    return planId;
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        private decimal CalculateCostPerSession(TreatmentPlanDto model)
        {
            if (model == null || model.Services == null)
                return 0;

            return model.Services.Sum(service => service.Cost ?? 0);
        }
    }
}
