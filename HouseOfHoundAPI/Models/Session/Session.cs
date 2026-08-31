using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Session
{
    public class Session
    {
        public class Booking
        {
            public int BookingId { get; set; }
            public int DogId { get; set; }
            public int TherapistId { get; set; }

            public DateTime StartTimeUtc { get; set; }
            public DateTime EndTimeUtc { get; set; }

            public string Status { get; set; } // Booked, Cancelled, Completed
            public string Notes { get; set; }
        }
    }
}