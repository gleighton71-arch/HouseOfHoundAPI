using HouseOfHoundAPI.Models.Metrics;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace HouseOfHound.Api.Repositories
{
    public class DogWeightRepository
    {
        public List<DogWeight> GetDogWeights(int dogId)
        {
            var weights = new List<DogWeight>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        DogId,
                        WeightKg,
                        RecordedDateUTC,
                        Note,
                        CreatedDateUTC
                    FROM dbo.DogWeight
                    WHERE DogId = @DogId
                    ORDER BY RecordedDateUTC DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", dogId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            weights.Add(MapDogWeight(reader));
                        }
                    }
                }
            }

            return weights;
        }

        public DogWeight GetDogWeightById(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        DogId,
                        WeightKg,
                        RecordedDateUTC,
                        Note,
                        CreatedDateUTC
                    FROM dbo.DogWeight
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDogWeight(reader);
                        }
                    }
                }
            }

            return null;
        }

        public DogWeight CreateDogWeight(DogWeight dogWeight)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.DogWeight
                    (
                        DogId,
                        WeightKg,
                        RecordedDateUTC,
                        Note,
                        CreatedDateUTC
                    )
                    OUTPUT
                        INSERTED.Id,
                        INSERTED.DogId,
                        INSERTED.WeightKg,
                        INSERTED.RecordedDateUTC,
                        INSERTED.Note,
                        INSERTED.CreatedDateUTC
                    VALUES
                    (
                        @DogId,
                        @WeightKg,
                        @RecordedDateUTC,
                        @Note,
                        SYSUTCDATETIME()
                    );", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", dogWeight.DogId);
                    cmd.Parameters.AddWithValue("@WeightKg", dogWeight.WeightKg);

                    cmd.Parameters.AddWithValue(
                        "@RecordedDateUTC",
                        dogWeight.RecordedDateUTC == default(DateTime)
                            ? DateTime.UtcNow
                            : dogWeight.RecordedDateUTC
                    );

                    cmd.Parameters.AddWithValue("@Note", (object)dogWeight.Note ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDogWeight(reader);
                        }
                    }
                }
            }

            throw new Exception("Failed to create dog weight record.");
        }

        public bool UpdateDogWeight(DogWeight dogWeight)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.DogWeight
                    SET
                        WeightKg = @WeightKg,
                        RecordedDateUTC = @RecordedDateUTC,
                        Note = @Note
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", dogWeight.Id);
                    cmd.Parameters.AddWithValue("@WeightKg", dogWeight.WeightKg);

                    cmd.Parameters.AddWithValue(
                        "@RecordedDateUTC",
                        dogWeight.RecordedDateUTC == default(DateTime)
                            ? DateTime.UtcNow
                            : dogWeight.RecordedDateUTC
                    );

                    cmd.Parameters.AddWithValue("@Note", (object)dogWeight.Note ?? DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteDogWeight(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    DELETE FROM dbo.DogWeight
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private DogWeight MapDogWeight(SqlDataReader reader)
        {
            return new DogWeight
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),

                DogId = reader.GetInt32(reader.GetOrdinal("DogId")),

                WeightKg = reader.GetDecimal(reader.GetOrdinal("WeightKg")),

                RecordedDateUTC = reader.GetDateTime(reader.GetOrdinal("RecordedDateUTC")),

                Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Note")),

                CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC"))
            };
        }
    }
}