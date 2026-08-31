using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Booking
{
    public class CreateBookingDto
    {
        public int DogId { get; set; }
        public int TherapistId { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } // Booked, Cancelled, Completed
        public decimal? Cost { get; set; } = default(decimal?);
        public int? InvoiceId { get; set; } // Optional, can be null if not invoiced yet
        public int? AppointmentTypeId { get; set; }
    }
}
