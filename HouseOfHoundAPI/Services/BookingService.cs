using HouseOfHoundAPI.Models.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class BookingService
    {
        private  DogService dogService = new DogService();
        private TherapistService therapistService = new TherapistService();


        public bool BookingExists(int bookingId)
        {
            using (var conn = Db.OpenConnection())
            {
              
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM Bookings WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                return result > 0;
            }
        }

        public bool BookingExists(int DogId,int TherapistId,DateTime start,DateTime end,out int Id)
        {
            using ( var conn = Db.OpenConnection())
            {
              
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT BookingId FROM Bookings WHERE DogId = @DogId AND TherapistId = @TherapistId AND StartTimeUtc < @EndTime AND EndTimeUtc > @StartTime";
                cmd.Parameters.AddWithValue("@DogId", DogId);
                cmd.Parameters.AddWithValue("@TherapistId", TherapistId);
                cmd.Parameters.AddWithValue("@StartTime", start);
                cmd.Parameters.AddWithValue("@EndTime", end);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                Id = result;
                return result > 0;
            }
        }


        public bool RemoveBooking(int bookingId)
        {

            if ( !BookingExists(bookingId))
            {
                return false;
            }
            using (var conn = Db.OpenConnection())
            {
               
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE FROM Bookings WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public BookingSummaryDto GetBookingSummary(int bookingId)
        {
            if (!BookingExists(bookingId))
            {
                return null;
            }
            using (var conn = Db.OpenConnection())
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT b.BookingId, d.Name AS DogName, o.Name AS OwnerName, t.Name AS TherapistName, b.StartTimeUtc, CASE WHEN b.EndTimeUtc < GETUTCDATE() THEN 'Completed' ELSE 'Upcoming' END AS Status,b.Cost
                                    FROM Bookings b
                                    JOIN Dogs d ON b.DogId = d.DogId
                                    JOIN Owners o ON d.OwnerId = o.OwnerId
                                    JOIN Therapists t ON b.TherapistId = t.TherapistId
                                    WHERE b.BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new BookingSummaryDto
                        {
                            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                            TherapistName = reader.GetString(reader.GetOrdinal("TherapistName")),
                            StartTimeUtc = reader.GetDateTime(reader.GetOrdinal("StartTimeUtc")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cost"))
                        };
                    }
                }
            }
            return null;
        }

        public void MarkBookingAsCancelled(int bookingId)
        {
            if (!BookingExists(bookingId))
            {
                return;
            }
            using (var conn = Db.OpenConnection())
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Bookings SET Status = 'Cancelled' WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkBookingAsCreated(int bookingId)
        {
            if (!BookingExists(bookingId))
            {
                return;
            }
            using (var conn = Db.OpenConnection())
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Bookings SET Status = 'Booked' WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
        }

        public void MarkBookingAsInProgress(int bookingId)
        {
            if (!BookingExists(bookingId))
            {
                return;
            }
            using (var conn = Db.OpenConnection())
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Bookings SET Status = 'In Progress' WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
        }


        public void MarkBookingAsCompleted(int bookingId)
        {
            if (!BookingExists(bookingId))
            {
                return;
            }
            using (var conn = Db.OpenConnection())
            {
                
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Bookings SET Status = 'Completed' WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
        }

        public List<Booking> GetBookingsForOwner(int ownerId)
        {
            var bookings = new List<Booking>();
            using (var conn = Db.OpenConnection())
            {
               
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT b.BookingId, b.DogId, b.TherapistId, b.StartTimeUtc, b.EndTimeUtc, b.Status, b.Notes, b.Cost, b.AppointmentTypeId
                                    FROM Bookings b
                                    JOIN Dogs d ON b.DogId = d.DogId
                                    WHERE d.OwnerId = @OwnerId";
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new Booking
                        {
                            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                            TherapistId = reader.GetInt32(reader.GetOrdinal("TherapistId")),
                            StartTimeUtc = reader.GetDateTime(reader.GetOrdinal("StartTimeUtc")),
                            EndTimeUtc = reader.GetDateTime(reader.GetOrdinal("EndTimeUtc")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                            Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cost")),
                            AppointmentTypeId = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AppointmentTypeId"))
                        });
                    }
                }
            }
            return bookings;
        }


        public int CreateBooking(CreateBookingDto createBooking)
        {
            if (createBooking == null || createBooking.DogId < 1 || createBooking.TherapistId < 1 || !createBooking.StartTimeUtc.HasValue || !createBooking.EndTimeUtc.HasValue)
            {
                return 0;
            }

            if ( createBooking.StartTimeUtc.Value >= createBooking.EndTimeUtc.Value)
            {
                return 0;
            }

            if (!dogService.DogExists(createBooking.DogId))
            {
                return 0;
            }
            if (!therapistService.TherapistExists(createBooking.TherapistId))
            {
                return 0;
            }

            if (BookingExists(createBooking.DogId, createBooking.TherapistId, createBooking.StartTimeUtc.Value, createBooking.EndTimeUtc.Value,out int id))
            {
                return id;
            }

            using (var conn = Db.OpenConnection())
            {
             
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Bookings (DogId, TherapistId, StartTimeUtc, EndTimeUtc, Notes, Status, Cost, InvoiceId, AppointmentTypeId) VALUES (@DogId, @TherapistId, @StartTimeUtc, @EndTimeUtc, @Notes, 'Booked', @Cost, @InvoiceId, @AppointmentTypeId); SELECT SCOPE_IDENTITY();";
                cmd.Parameters.AddWithValue("@DogId", createBooking.DogId);
                cmd.Parameters.AddWithValue("@TherapistId", createBooking.TherapistId);
                cmd.Parameters.AddWithValue("@StartTimeUtc", createBooking.StartTimeUtc.Value);
                cmd.Parameters.AddWithValue("@EndTimeUtc", createBooking.EndTimeUtc.Value);
                cmd.Parameters.AddWithValue("@Notes", createBooking.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Cost", createBooking.Cost ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InvoiceId",createBooking.InvoiceId ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AppointmentTypeId", createBooking.AppointmentTypeId ?? (object)DBNull.Value);
                int newId = Convert.ToInt32(cmd.ExecuteScalar());
                return newId;
            }

        }

        public bool UpdateBooking(int id, CreateBookingDto dto)
        {
            // update booking with id to have the values in dto
            if (!BookingExists(id))
            {
                return false;
            }


            using (var conn = Db.OpenConnection())
            {

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Bookings SET DogId = @DogId, TherapistId = @TherapistId, StartTimeUtc = @StartTimeUtc, EndTimeUtc = @EndTimeUtc, Notes = @Notes, Status = @Status, Cost = @Cost, AppointmentTypeId = @AppointmentTypeId WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", id);
                cmd.Parameters.AddWithValue("@DogId", dto.DogId);
                cmd.Parameters.AddWithValue("@TherapistId", dto.TherapistId);
                cmd.Parameters.AddWithValue("@StartTimeUtc", dto.StartTimeUtc.Value);
                cmd.Parameters.AddWithValue("@EndTimeUtc", dto.EndTimeUtc.Value);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(dto.Status) ? "Booked" : dto.Status);
                cmd.Parameters.AddWithValue("@Notes", dto.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Cost", dto.Cost ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AppointmentTypeId", dto.AppointmentTypeId ?? (object)DBNull.Value);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }

        }

        public List<BookingDisplay> GetAllBookings(DateTime? day)
        {
            var bookings = new List<BookingDisplay>();

            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT b.BookingId, b.DogId, d.Name AS DogName, o.FullName AS OwnerName, b.TherapistId, t.Name AS TherapistName, b.StartTimeUtc, b.EndTimeUtc, b.Status, b.Notes, b.Cost, b.CreatedUTC, b.AppointmentTypeId, at.Code AS AppointmentTypeCode, at.Description AS AppointmentTypeDescription
                                    FROM Bookings b
                                    JOIN Dogs d ON b.DogId = d.DogId
                                    JOIN Owners o ON d.OwnerId = o.OwnerId
                                    JOIN Therapists t ON b.TherapistId = t.TherapistId
                                    LEFT JOIN AppointmentTypes at ON at.Id = b.AppointmentTypeId";
                if (day.HasValue)
                {
                    cmd.CommandText += " WHERE CAST(b.StartTimeUtc AS DATE) = @Day";
                    cmd.Parameters.AddWithValue("@Day", day.Value.Date);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new BookingDisplay
                        {
                            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                            DogId = reader.GetInt32(reader.GetOrdinal("DogId")),
                            DogName = reader.GetString(reader.GetOrdinal("DogName")),
                            OwnerName = reader.GetString(reader.GetOrdinal("OwnerName")),
                            TherapistId = reader.GetInt32(reader.GetOrdinal("TherapistId")),
                            TherapistName = reader.GetString(reader.GetOrdinal("TherapistName")),
                            StartTimeUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("StartTimeUtc")), DateTimeKind.Utc),
                            EndTimeUtc = DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("EndTimeUtc")), DateTimeKind.Utc),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                            Cost = reader.IsDBNull(reader.GetOrdinal("Cost")) ? (decimal?)null : reader.GetDecimal(reader.GetOrdinal("Cost")),
                            CreatedUTC = reader.IsDBNull(reader.GetOrdinal("CreatedUTC")) ? (DateTime?)null : DateTime.SpecifyKind(reader.GetDateTime(reader.GetOrdinal("CreatedUTC")), DateTimeKind.Utc),
                            AppointmentTypeId = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("AppointmentTypeId")),
                            AppointmentTypeCode = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeCode")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeCode")),
                            AppointmentTypeDescription = reader.IsDBNull(reader.GetOrdinal("AppointmentTypeDescription")) ? null : reader.GetString(reader.GetOrdinal("AppointmentTypeDescription"))
                        });
                    }
                }
            }


            return bookings;

        }
    }
}
