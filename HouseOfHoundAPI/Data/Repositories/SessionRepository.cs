using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Models.Session;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

public class SessionRepository
{
    private readonly string _connectionString;

    public SessionRepository()
    {
        _connectionString = HohManager.GetConnectionString();
    }

    public int CreateSession(SqlConnection conn, SqlTransaction tx, CreateSessionDto dto)
    {
        var sessionDate = dto.SessionDateUtc ?? DateTime.UtcNow;

        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Sessions (BookingId, SessionDateUtc, ClinicalNotes)
OUTPUT INSERTED.SessionId
VALUES (@BookingId, @SessionDateUtc, @ClinicalNotes);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@BookingId", dto.BookingId);
            cmd.Parameters.AddWithValue("@SessionDateUtc", sessionDate);
            cmd.Parameters.AddWithValue("@ClinicalNotes", (object)dto.ClinicalNotes ?? DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }
    }

    public List<SessionWorkItemDto> GetTherapistWorklist(int therapistId, DateTime day)
    {
        var items = new List<SessionWorkItemDto>();
        var start = day.Date;
        var end = start.AddDays(1);

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
SELECT
    b.BookingId,
    b.DogId,
    d.OwnerId,
    d.Name AS DogName,
    d.Breed,
    d.DateOfBirth,
    d.ImageURL,
    b.TherapistId,
    t.Name AS TherapistName,
    b.StartTimeUtc,
    b.EndTimeUtc,
    b.Status,
    b.Notes,
    b.Cost,
    b.AppointmentTypeId,
    at.Code AS AppointmentTypeCode,
    at.Description AS AppointmentTypeDescription,
    b.InvoiceId,
    i.Status AS InvoiceStatus,
    i.TotalAmount AS InvoiceTotalAmount
FROM dbo.Bookings b
INNER JOIN dbo.Dogs d ON d.DogId = b.DogId
INNER JOIN dbo.Therapists t ON t.TherapistId = b.TherapistId
LEFT JOIN dbo.Invoices i ON i.InvoiceId = b.InvoiceId
LEFT JOIN dbo.AppointmentTypes at ON at.Id = b.AppointmentTypeId
WHERE b.TherapistId = @TherapistId
  AND b.StartTimeUtc >= @StartTimeUtc
  AND b.StartTimeUtc < @EndTimeUtc
  AND b.Status <> N'Cancelled'
ORDER BY b.StartTimeUtc;", conn))
        {
            cmd.Parameters.AddWithValue("@TherapistId", therapistId);
            cmd.Parameters.AddWithValue("@StartTimeUtc", start);
            cmd.Parameters.AddWithValue("@EndTimeUtc", end);

            conn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var invoiceStatus = reader.IsDBNull(reader.GetOrdinal("InvoiceStatus"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("InvoiceStatus"));

                    items.Add(new SessionWorkItemDto
                    {
                        BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                        DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                        OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId")),
                        DogName = reader.IsDBNull(reader.GetOrdinal("DogName")) ? null : reader.GetString(reader.GetOrdinal("DogName")),
                        Breed = reader.IsDBNull(reader.GetOrdinal("Breed")) ? null : reader.GetString(reader.GetOrdinal("Breed")),
                        DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                        ImageURL = reader.IsDBNull(reader.GetOrdinal("ImageURL")) ? null : reader.GetString(reader.GetOrdinal("ImageURL")),
                        TherapistId = reader.GetInt32(reader.GetOrdinal("TherapistId")),
                        TherapistName = reader.IsDBNull(reader.GetOrdinal("TherapistName")) ? null : reader.GetString(reader.GetOrdinal("TherapistName")),
                        StartTimeUtc = reader.GetDateTime(reader.GetOrdinal("StartTimeUtc")),
                        EndTimeUtc = reader.GetDateTime(reader.GetOrdinal("EndTimeUtc")),
                        Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? null : reader.GetString(reader.GetOrdinal("Status")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                        Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cost")),
                        AppointmentTypeId = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AppointmentTypeId")),
                        AppointmentTypeCode = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeCode")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeCode")),
                        AppointmentTypeDescription = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeDescription")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeDescription")),
                        InvoiceId = reader.IsDBNull(reader.GetOrdinal("InvoiceId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                        InvoiceStatus = invoiceStatus,
                        InvoiceTotalAmount = reader.IsDBNull(reader.GetOrdinal("InvoiceTotalAmount")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("InvoiceTotalAmount")),
                        IsPaid = string.Equals(invoiceStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
        }

        return items;
    }

    public bool UpdateBookingStatus(int bookingId, string status)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
UPDATE dbo.Bookings
SET Status = @Status
WHERE BookingId = @BookingId;", conn))
        {
            cmd.Parameters.AddWithValue("@BookingId", bookingId);
            cmd.Parameters.AddWithValue("@Status", status);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
