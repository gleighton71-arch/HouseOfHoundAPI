using HouseOfHoundAPI.Models.Equipment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class EquipmentService
    {
        // 🔹 GET ALL (optionally only active)
        public List<Equipment> GetEquipment(bool activeOnly = false)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            SELECT 
                EquipmentId,
                Name,
                Category,
                HasValue,
                Value,
                SerialNumber,
                Status,
                Active,
                CreatedDate
            FROM Equipment
            WHERE (@activeOnly = 0 OR Active = 1)", conn))
            {
                cmd.Parameters.AddWithValue("@activeOnly", activeOnly);

                var list = new List<Equipment>();

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(MapEquipment(rdr));
                    }
                }

                HydrateServiceSchedules(conn, list);
                return list;
            }
        }

        // 🔹 GET BY ID
        public Equipment GetEquipmentById(int id)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            SELECT 
                EquipmentId,
                Name,
                Category,
                HasValue,
                Value,
                SerialNumber,
                Status,
                Active,
                CreatedDate
            FROM Equipment
            WHERE EquipmentId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                Equipment item = null;

                using (var rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        item = MapEquipment(rdr);
                    }
                }

                if (item != null)
                {
                    HydrateServiceSchedules(conn, new List<Equipment> { item });
                }

                return item;
            }
        }

        // 🔹 INSERT
        public int InsertEquipment(Equipment model)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            INSERT INTO Equipment (
                Name,
                Category,
                HasValue,
                Value,
                SerialNumber
            )
            OUTPUT INSERTED.EquipmentId
            VALUES (
                @Name,
                @Category,
                @HasValue,
                @Value,
                @SerialNumber
            )", conn))
            {
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Category", (object)model.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HasValue", model.HasValue);
                cmd.Parameters.AddWithValue("@Value", (object)model.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SerialNumber", (object)model.SerialNumber ?? DBNull.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        // 🔹 UPDATE
        public void UpdateEquipment(Equipment model)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            UPDATE Equipment SET
                Name = @Name,
                Category = @Category,
                HasValue = @HasValue,
                Value = @Value,
                SerialNumber = @SerialNumber,
                Status = @Status,
                Active = @Active
            WHERE EquipmentId = @EquipmentId", conn))
            {
                cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
                cmd.Parameters.AddWithValue("@Name", model.Name);
                cmd.Parameters.AddWithValue("@Category", (object)model.Category ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HasValue", model.HasValue);
                cmd.Parameters.AddWithValue("@Value", (object)model.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SerialNumber", (object)model.SerialNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", model.Status);
                cmd.Parameters.AddWithValue("@Active", model.Active);

                cmd.ExecuteNonQuery();
            }
        }

        // 🔹 DELETE (hard delete — optional, use with care)
        public void DeleteEquipment(int id)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand("DELETE FROM Equipment WHERE EquipmentId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // 🔹 SOFT DELETE (recommended)
        public void DeactivateEquipment(int id)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            UPDATE Equipment 
            SET Active = 0 
            WHERE EquipmentId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public int InsertServiceSchedule(CreateEquipmentServiceDto model)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            INSERT INTO EquipmentServiceSchedules (
                EquipmentId,
                ServiceName,
                ServiceInterval,
                ServiceDate,
                Status,
                ServiceDueDate,
                BookedServiceDate,
                Notes
            )
            OUTPUT INSERTED.EquipmentServiceScheduleId
            VALUES (
                @EquipmentId,
                @ServiceName,
                @ServiceInterval,
                @ServiceDate,
                @Status,
                @ServiceDueDate,
                @BookedServiceDate,
                @Notes
            )", conn))
            {
                cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
                cmd.Parameters.AddWithValue("@ServiceName", (object)model.ServiceName ?? "");
                cmd.Parameters.AddWithValue("@ServiceInterval", GetServiceInterval(model.ServiceInterval));
                cmd.Parameters.AddWithValue("@ServiceDate", (object)model.ServiceDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", model.Status);
                cmd.Parameters.AddWithValue("@ServiceDueDate", model.ServiceDueDate.Date);
                cmd.Parameters.AddWithValue("@BookedServiceDate", (object)model.BookedServiceDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", (object)model.Notes ?? DBNull.Value);

                return (int)cmd.ExecuteScalar();
            }
        }

        public bool UpdateServiceSchedule(int scheduleId, CreateEquipmentServiceDto model)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            UPDATE EquipmentServiceSchedules SET
                ServiceName = @ServiceName,
                ServiceInterval = @ServiceInterval,
                ServiceDate = @ServiceDate,
                Status = @Status,
                ServiceDueDate = @ServiceDueDate,
                BookedServiceDate = @BookedServiceDate,
                Notes = @Notes
            WHERE EquipmentServiceScheduleId = @ScheduleId
              AND EquipmentId = @EquipmentId", conn))
            {
                cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                cmd.Parameters.AddWithValue("@EquipmentId", model.EquipmentId);
                cmd.Parameters.AddWithValue("@ServiceName", (object)model.ServiceName ?? "");
                cmd.Parameters.AddWithValue("@ServiceInterval", GetServiceInterval(model.ServiceInterval));
                cmd.Parameters.AddWithValue("@ServiceDate", (object)model.ServiceDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", model.Status);
                cmd.Parameters.AddWithValue("@ServiceDueDate", model.ServiceDueDate.Date);
                cmd.Parameters.AddWithValue("@BookedServiceDate", (object)model.BookedServiceDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", (object)model.Notes ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public List<EquipmentServiceSchedule> GetDueUnbookedServiceSchedules(DateTime fromDate, DateTime toDate)
        {
            using (var conn = Db.OpenConnection())
            using (var cmd = new SqlCommand(@"
            SELECT
                s.EquipmentServiceScheduleId,
                s.EquipmentId,
                s.ServiceName,
                s.ServiceInterval,
                s.ServiceDate,
                s.Status,
                s.ServiceDueDate,
                s.BookedServiceDate,
                s.Notes
            FROM EquipmentServiceSchedules s
            JOIN Equipment e ON e.EquipmentId = s.EquipmentId
            WHERE e.Active = 1
              AND s.ServiceDueDate >= @FromDate
              AND s.ServiceDueDate < @ToDate
              AND s.BookedServiceDate IS NULL
              AND s.Status = 'Service Due'
            ORDER BY s.ServiceDueDate ASC, s.EquipmentId ASC", conn))
            {
                cmd.Parameters.AddWithValue("@FromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@ToDate", toDate.Date);

                var list = new List<EquipmentServiceSchedule>();
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(MapServiceSchedule(rdr));
                    }
                }

                return list;
            }
        }

        // 🔹 PRIVATE MAPPER (keeps things tidy)
        private Equipment MapEquipment(SqlDataReader rdr)
        {
            return new Equipment
            {
                EquipmentId = rdr.GetInt32(rdr.GetOrdinal("EquipmentId")),
                Name = rdr.GetString(rdr.GetOrdinal("Name")),
                Category = rdr["Category"] as string,
                HasValue = rdr.GetBoolean(rdr.GetOrdinal("HasValue")),
                Value = rdr["Value"] != DBNull.Value ? (decimal?)rdr["Value"] : null,
                SerialNumber = rdr["SerialNumber"] as string,
                Status = rdr.GetString(rdr.GetOrdinal("Status")),
                Active = rdr.GetBoolean(rdr.GetOrdinal("Active")),
                CreatedDate = rdr.GetDateTime(rdr.GetOrdinal("CreatedDate"))
            };
        }

        private void HydrateServiceSchedules(SqlConnection conn, List<Equipment> equipment)
        {
            if (equipment == null || equipment.Count == 0) return;

            var ids = equipment.Select(e => e.EquipmentId).ToList();
            var parameterNames = ids.Select((id, index) => "@id" + index).ToList();

            using (var cmd = new SqlCommand(@"
            SELECT
                EquipmentServiceScheduleId,
                EquipmentId,
                ServiceName,
                ServiceInterval,
                ServiceDate,
                Status,
                ServiceDueDate,
                BookedServiceDate,
                Notes
            FROM EquipmentServiceSchedules
            WHERE EquipmentId IN (" + string.Join(",", parameterNames) + @")
            ORDER BY ServiceDueDate DESC, EquipmentServiceScheduleId DESC", conn))
            {
                for (var i = 0; i < ids.Count; i++)
                {
                    cmd.Parameters.AddWithValue(parameterNames[i], ids[i]);
                }

                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var equipmentId = rdr.GetInt32(rdr.GetOrdinal("EquipmentId"));
                        var item = equipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
                        if (item != null)
                        {
                            item.ServiceSchedules.Add(MapServiceSchedule(rdr));
                        }
                    }
                }
            }
        }

        private EquipmentServiceSchedule MapServiceSchedule(SqlDataReader rdr)
        {
            return new EquipmentServiceSchedule
            {
                EquipmentServiceScheduleId = rdr.GetInt32(rdr.GetOrdinal("EquipmentServiceScheduleId")),
                EquipmentId = rdr.GetInt32(rdr.GetOrdinal("EquipmentId")),
                ServiceName = rdr["ServiceName"] as string,
                ServiceInterval = rdr["ServiceInterval"] as string,
                ServiceDate = rdr["ServiceDate"] != DBNull.Value ? (DateTime?)rdr["ServiceDate"] : null,
                Status = rdr.GetString(rdr.GetOrdinal("Status")),
                ServiceDueDate = rdr.GetDateTime(rdr.GetOrdinal("ServiceDueDate")),
                BookedServiceDate = rdr["BookedServiceDate"] != DBNull.Value ? (DateTime?)rdr["BookedServiceDate"] : null,
                Notes = rdr["Notes"] as string
            };
        }

        private string GetServiceInterval(string value)
        {
            switch ((value ?? "").Trim().ToLower())
            {
                case "daily":
                    return "Daily";
                case "weekly":
                    return "Weekly";
                case "yearly":
                case "annually":
                    return "Yearly";
                case "monthly":
                case "month":
                default:
                    return "Monthly";
            }
        }
    }
}
