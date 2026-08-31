using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Http;

public class BookingRepository
{
    public int CreateBooking(SqlConnection conn, SqlTransaction tx, CreateBookingDto dto)
    {
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Bookings (DogId, TherapistId, StartTimeUtc, EndTimeUtc, Status, Notes, AppointmentTypeId)
OUTPUT INSERTED.BookingId
VALUES (@DogId, @TherapistId, @Start, @End, @Status, @Notes, @AppointmentTypeId);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@DogId", dto.DogId);
            cmd.Parameters.AddWithValue("@TherapistId", dto.TherapistId);
            cmd.Parameters.AddWithValue("@Start", dto.StartTimeUtc);
            cmd.Parameters.AddWithValue("@End", dto.EndTimeUtc);
            cmd.Parameters.AddWithValue("@Status", "Booked");
            cmd.Parameters.AddWithValue("@Notes", (object)dto.Notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AppointmentTypeId", dto.AppointmentTypeId ?? (object)DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }
    }

    public bool BookingExists(SqlConnection conn, SqlTransaction tx, int bookingId)
    {
        using (var cmd = new SqlCommand("SELECT 1 FROM dbo.Bookings WHERE BookingId = @BookingId;", conn, tx))
        {
            cmd.Parameters.AddWithValue("@BookingId", bookingId);
            return cmd.ExecuteScalar() != null;
        }
    }

    public async Task<List<DateTime>> GetAvailableAppointmentTimesAsync(
    int therapistId,
    int dogId,
    int durationMinutes,
    DateTime appointmentDate,
    int slotIntervalMinutes = 15)
    {
        if (durationMinutes < 25 || durationMinutes > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                "Appointment duration must be between 25 and 90 minutes.");
        }

        // No Sunday appointments
        if (appointmentDate.DayOfWeek == DayOfWeek.Sunday)
        {
            return new List<DateTime>();
        }

        TimeSpan openingTime;
        TimeSpan closingTime;

        if (appointmentDate.DayOfWeek == DayOfWeek.Saturday)
        {
            openingTime = new TimeSpan(9, 30, 0);
            closingTime = new TimeSpan(14, 30, 0);
        }
        else
        {
            openingTime = new TimeSpan(8, 30, 0);
            closingTime = new TimeSpan(17, 30, 0);
        }

        /*
            Assuming appointmentDate represents a UK local date.

            Build the local opening/closing times and convert them to UTC,
            because the database stores UTC values.
        */

        var localDayStart = DateTime.SpecifyKind(
     appointmentDate.Date.Add(openingTime),
     DateTimeKind.Unspecified);

        var localDayEnd = DateTime.SpecifyKind(
            appointmentDate.Date.Add(closingTime),
            DateTimeKind.Unspecified);

        var ukTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        var dayStartUtc =
            TimeZoneInfo.ConvertTimeToUtc(localDayStart, ukTimeZone);

        var dayEndUtc =
            TimeZoneInfo.ConvertTimeToUtc(localDayEnd, ukTimeZone);

        /*
            Only retrieve bookings that might overlap the working day.

            Cancelled bookings do not block availability.
        */

        BookingService bookingService = new BookingService();

        List<BookingDisplay> currentBookings = bookingService.GetAllBookings(localDayStart);
        var existingBookings = currentBookings
            .Where(b =>
                b.Status != "Cancelled" &&
                (b.TherapistId == therapistId || b.DogId == dogId) &&
                b.StartTimeUtc < dayEndUtc &&
                b.EndTimeUtc > dayStartUtc)
            .Select(b => new
            {
                b.StartTimeUtc,
                b.EndTimeUtc
            })
            .ToList();

        var availableTimes = new List<DateTime>();

        for (
            var candidateStartUtc = dayStartUtc;
            candidateStartUtc.AddMinutes(durationMinutes) <= dayEndUtc;
            candidateStartUtc = candidateStartUtc.AddMinutes(slotIntervalMinutes))
        {
            var candidateEndUtc = candidateStartUtc.AddMinutes(durationMinutes);

            var overlaps = existingBookings.Any(b =>
                candidateStartUtc < b.EndTimeUtc &&
                candidateEndUtc > b.StartTimeUtc);

            if (!overlaps)
            {
                availableTimes.Add(candidateStartUtc);
            }
        }

        return availableTimes;
    }
}
