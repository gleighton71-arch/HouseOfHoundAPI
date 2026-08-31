
using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace HouseOfHound.Api.Repositories
{
    public class DogImageRepository
    {
        public List<DogImage> GetDogImages(int dogId)
        {
            var images = new List<DogImage>();

            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        DogId,
                        FileName,
                        OriginalFileName,
                        FilePath,
                        ContentType,
                        FileSizeBytes,
                        Note,
                        UploadedDateUTC,
                        IsActive
                    FROM dbo.DogImage
                    WHERE DogId = @DogId
                      AND IsActive = 1
                    ORDER BY UploadedDateUTC DESC;", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", dogId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            images.Add(MapDogImage(reader));
                        }
                    }
                }
            }

            return images;
        }

        public DogImage GetDogImageById(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    SELECT
                        Id,
                        DogId,
                        FileName,
                        OriginalFileName,
                        FilePath,
                        ContentType,
                        FileSizeBytes,
                        Note,
                        UploadedDateUTC,
                        IsActive
                    FROM dbo.DogImage
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDogImage(reader);
                        }
                    }
                }
            }

            return null;
        }

        public DogImage CreateDogImage(DogImage dogImage)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    INSERT INTO dbo.DogImage
                    (
                        DogId,
                        FileName,
                        OriginalFileName,
                        FilePath,
                        ContentType,
                        FileSizeBytes,
                        Note,
                        UploadedDateUTC,
                        IsActive
                    )
                    OUTPUT
                        INSERTED.Id,
                        INSERTED.DogId,
                        INSERTED.FileName,
                        INSERTED.OriginalFileName,
                        INSERTED.FilePath,
                        INSERTED.ContentType,
                        INSERTED.FileSizeBytes,
                        INSERTED.Note,
                        INSERTED.UploadedDateUTC,
                        INSERTED.IsActive
                    VALUES
                    (
                        @DogId,
                        @FileName,
                        @OriginalFileName,
                        @FilePath,
                        @ContentType,
                        @FileSizeBytes,
                        @Note,
                        SYSUTCDATETIME(),
                        1
                    );", conn))
                {
                    cmd.Parameters.AddWithValue("@DogId", dogImage.DogId);
                    cmd.Parameters.AddWithValue("@FileName", dogImage.FileName);
                    cmd.Parameters.AddWithValue("@OriginalFileName", (object)dogImage.OriginalFileName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FilePath", dogImage.FilePath);
                    cmd.Parameters.AddWithValue("@ContentType", (object)dogImage.ContentType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileSizeBytes", (object)dogImage.FileSizeBytes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Note", (object)dogImage.Note ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapDogImage(reader);
                        }
                    }
                }
            }

            throw new Exception("Failed to create dog image.");
        }

        public bool UpdateDogImageNote(int id, string note)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.DogImage
                    SET Note = @Note
                    WHERE Id = @Id
                      AND IsActive = 1;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Note", (object)note ?? DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool SoftDeleteDogImage(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    UPDATE dbo.DogImage
                    SET IsActive = 0
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteDogImage(int id)
        {
            using (var conn = HohManager.GetOpenConnection())
            {
                using (var cmd = new SqlCommand(@"
                    DELETE FROM dbo.DogImage
                    WHERE Id = @Id;", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        private DogImage MapDogImage(SqlDataReader reader)
        {
            return new DogImage
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                OriginalFileName = reader.IsDBNull(reader.GetOrdinal("OriginalFileName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("OriginalFileName")),
                FilePath = reader.GetString(reader.GetOrdinal("FilePath")),
                ContentType = reader.IsDBNull(reader.GetOrdinal("ContentType"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ContentType")),
                FileSizeBytes = reader.IsDBNull(reader.GetOrdinal("FileSizeBytes"))
                    ? (long?)null
                    : reader.GetInt64(reader.GetOrdinal("FileSizeBytes")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Note")),
                UploadedDateUTC = reader.GetDateTime(reader.GetOrdinal("UploadedDateUTC")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}