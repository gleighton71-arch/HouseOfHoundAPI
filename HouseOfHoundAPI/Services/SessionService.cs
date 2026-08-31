using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class SessionService
    {
        private  DogService dogService = new DogService();
        private TherapistService therapistService = new TherapistService();


        public bool SessionExists(int bookingId)
        {
            using (var conn = Db.OpenConnection())
            {
              
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM Sessions WHERE BookingId = @BookingId";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                return result > 0;
            }
        }


        public bool RemoveSession(int Id)
        {

            if ( !SessionExists(Id))
            {
                return true;
            }
            using (var conn = Db.OpenConnection())
            {
               
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE FROM Sessions WHERE SessionId = @Id";
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.ExecuteNonQuery();
            }
            return true;
        }



        public int CreateSession(CreateSessionDto createSession)
        {
            if (createSession == null || createSession.BookingId < 1 || !createSession.SessionDateUtc.HasValue)
            {
                return 0;
            }

            if (SessionExists(createSession.BookingId))
            {
                return 0;
            }

            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Sessions (BookingId, SessionDateUtc, ClinicalNotes) 
                                    VALUES (@BookingId, @SessionDateUtc, @ClinicalNotes);
                                    SELECT SCOPE_IDENTITY();";
                cmd.Parameters.AddWithValue("@BookingId", createSession.BookingId);
                cmd.Parameters.AddWithValue("@SessionDateUtc", createSession.SessionDateUtc.Value);
                cmd.Parameters.AddWithValue("@ClinicalNotes", createSession.ClinicalNotes ?? (object)DBNull.Value);
                int newSessionId = Convert.ToInt32(cmd.ExecuteScalar());
                return newSessionId;
            }
        }

        public List<SessionDetailDto> GetSessionsForBooking(int bookingId)
        {
            // get session details for a booking
            List<SessionDetailDto> sessions = new List<SessionDetailDto>();

            using (var conn = Db.OpenConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"select BookingId, SessionDateUtc, ClinicalNotes 
                                    from Sessions where BookingId = @BookingId";
                                    
                cmd.Parameters.AddWithValue("@BookingId", bookingId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sessions.Add(new SessionDetailDto
                        {
                            BookingId = reader.GetInt32(reader.GetOrdinal("BookingId")),
                            SessionDateUtc = reader.GetDateTime(reader.GetOrdinal("SessionDateUtc")),
                            ClinicalNotes = reader.IsDBNull(reader.GetOrdinal("ClinicalNotes")) ? null : reader.GetString(reader.GetOrdinal("ClinicalNotes")),
                            Media = new List<SessionMediaDto>()
                        });
                    }
                }
            }
            return sessions;
        }
    }
}