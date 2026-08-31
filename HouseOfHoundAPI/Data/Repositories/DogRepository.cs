using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

public class DogRepository
{
    private SqlConnection _conn = HohManager.GetOpenConnection();

    public List<DogDetailDto> GetDogs()
    {
        var dogs = new List<DogDetailDto>();
            using (var cmd = new SqlCommand("SELECT DogId, OwnerId, Name, Breed, DateOfBirth, WeightKg, Notes,ImageURL, MicroChip, IsVetReferral, IsArchived FROM dbo.Dogs;", _conn))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dogs.Add(new DogDetailDto
                    {
                        DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                        OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Breed = reader.IsDBNull(reader.GetOrdinal("Breed")) ? null : reader.GetString(reader.GetOrdinal("Breed")),
                        DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        WeightKg = reader.IsDBNull(reader.GetOrdinal("WeightKg")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("WeightKg")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                        ImageURL = reader.IsDBNull(reader.GetOrdinal("ImageURL")) ? null : reader.GetString(reader.GetOrdinal("ImageURL")),
                        MicroChip = reader.IsDBNull(reader.GetOrdinal("MicroChip")) ? null : reader.GetString(reader.GetOrdinal("MicroChip")),
                        IsVetReferral = reader.GetBoolean(reader.GetOrdinal("IsVetReferral")),
                        IsArchived = reader.GetBoolean(reader.GetOrdinal("IsArchived"))

                    });
                }
            }
        
        return dogs;
    }


    public int UpdateDog(int dogId, CreateDogDto dto)
    {
        using (var conn = HohManager.GetOpenConnection())
        {
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int? ownerId = GetOwnerIdForDog(conn, tx, dogId);
                    if (ownerId == null)
                        throw new Exception("Dog not found");
                    if (ownerId != dto.OwnerId)
                        throw new Exception("Cannot change dog ownership");
                    int rowsAffected = UpdateDog(conn, tx, dogId, dto);
                    tx.Commit();
                    return rowsAffected;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }

    public int UpdateDog(SqlConnection conn, SqlTransaction tx, int dogId, CreateDogDto dto)
    {
        using (var cmd = new SqlCommand(@"
UPDATE dbo.Dogs
SET Name = @Name,
    Breed = @Breed,
    DateOfBirth = @DateOfBirth,
    WeightKg = @WeightKg,
    ImageURL = @ImageURL,
    MicroChip = @MicroChip,
    IsVetReferral = @IsVetReferral,
    IsArchived = @IsArchived
   
WHERE DogId = @DogId;", conn, tx))
        {
            cmd.Parameters.AddWithValue("@DogId", dogId);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Breed", (object)dto.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object)dto.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WeightKg", (object)dto.WeightKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImageURL", (object)dto.ImageURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MicroChip", (object)dto.MicroChip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsVetReferral", dto.IsVetReferral);
            cmd.Parameters.AddWithValue("@IsArchived", dto.IsArchived);

            return cmd.ExecuteNonQuery();
        }
    }
    public int CreateDog(CreateDogDto dto)
    {
        using (var conn = HohManager.GetOpenConnection())
        {
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    int newDogId = CreateDog(conn, tx, dto);
                    tx.Commit();
                    return newDogId;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }

    public int CreateDog(SqlConnection conn, SqlTransaction tx, CreateDogDto dto)
    {
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Dogs (OwnerId, Name, Breed, DateOfBirth, WeightKg, Notes, ImageURL, MicroChip, IsVetReferral, IsArchived)
OUTPUT INSERTED.DogId
VALUES (@OwnerId, @Name, @Breed, @DateOfBirth, @WeightKg, @Notes, @ImageURL, @MicroChip, @IsVetReferral, @IsArchived);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@OwnerId", dto.OwnerId);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Breed", (object)dto.Breed ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateOfBirth", (object)dto.DateOfBirth ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WeightKg", (object)dto.WeightKg ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Notes", (object)dto.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImageURL", (object)dto.ImageURL ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MicroChip", (object)dto.MicroChip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsVetReferral", dto.IsVetReferral);
            cmd.Parameters.AddWithValue("@IsArchived", dto.IsArchived);

            return (int)cmd.ExecuteScalar();
        }
    }

    public int? GetOwnerIdForDog(SqlConnection conn, SqlTransaction tx, int dogId)
    {
        using (var cmd = new SqlCommand("SELECT OwnerId FROM dbo.Dogs WHERE DogId = @DogId;", conn, tx))
        {
            cmd.Parameters.AddWithValue("@DogId", dogId);
            var result = cmd.ExecuteScalar();
            return result == null ? (int?)null : Convert.ToInt32(result);
        }
    }

    public List<DogDetailDto> GetDogsByOwnerId(int ownerId)
    {
        List<DogDetailDto> dogList = GetDogs();


        return dogList.Where(d => d.OwnerId == ownerId).ToList();

    }

    public List<Note> GetDogNotes(int dogId)
    {
        var notes = new List<Note>();

        using (var conn = HohManager.GetOpenConnection())
        {
            using (var cmd = new SqlCommand(@"
        SELECT 
            Id,
            DogId,
            Content,
            CreatedDateUTC,
            RequiresAction
        FROM dbo.Note
        WHERE DogId = @DogId
        ORDER BY CreatedDateUTC DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@DogId", dogId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notes.Add(new Note
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                            Content = reader.GetString(reader.GetOrdinal("Content")),
                            CreatedDateUTC = reader.GetDateTime(reader.GetOrdinal("CreatedDateUTC")),
                            RequiresAction = reader.GetBoolean(reader.GetOrdinal("RequiresAction"))
                        });
                    }
                }
            }
        }

        return notes;
    }


    public List<BookingDisplay> GetDogBookings(int dogId)
    {
        var bookings = new List<BookingDisplay>();
        using (var conn = HohManager.GetOpenConnection())
        {
            using (var cmd = new SqlCommand(@"
        SELECT 
            b.BookingId,
            b.DogId,
            t.TherapistId,
            t.Name AS TherapistName,
            b.StartTimeUtc,
            b.EndTimeUtc,
            b.Status,
            b.Notes,
            b.Cost,
            b.CreatedUTC,
            b.AppointmentTypeId,
            at.Code AS AppointmentTypeCode,
            at.Description AS AppointmentTypeDescription
        FROM dbo.Bookings b
        JOIN dbo.Therapists t ON b.TherapistId = t.TherapistId
        LEFT JOIN dbo.AppointmentTypes at ON at.Id = b.AppointmentTypeId
        WHERE b.DogId = @DogId
        ORDER BY b.StartTimeUtc DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@DogId", dogId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new BookingDisplay
                        {
                            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                            TherapistId = reader.GetInt32(reader.GetOrdinal("TherapistId")),
                            StartTimeUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("StartTimeUtc")),DateTimeKind.Utc),
                            EndTimeUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("EndTimeUtc")),DateTimeKind.Utc),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                            TherapistName = reader.GetString(reader.GetOrdinal("TherapistName")),
                            Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cost")),
                            CreatedUTC = reader.IsDBNull(reader.GetOrdinal("CreatedUTC")) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedUTC")), DateTimeKind.Utc),
                            AppointmentTypeId = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AppointmentTypeId")),
                            AppointmentTypeCode = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeCode")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeCode")),
                            AppointmentTypeDescription = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeDescription")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeDescription"))
                        });
                    }
                }
            }
        }
        return bookings;
    }
}
