using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Booking
{
    public class BookingSummaryDto
    {
        public int BookingId { get; set; }
        public string DogName { get; set; }
        public string OwnerName { get; set; }
        public string TherapistName { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public string Status { get; set; }
        public decimal? Cost { get; set; } = default(decimal?);
    }
}