using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Booking
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
        public decimal? Cost { get; set; } = default(decimal?);
        public int? AppointmentTypeId { get; set; }
        public DateTime? CreatedUTC { get; set; }
    }

    public class BookingDisplay : Booking
    {
        public string DogName { get; set; }
        public string OwnerName { get; set; }
        public string TherapistName { get; set; }
        public string AppointmentTypeCode { get; set; }
        public string AppointmentTypeDescription { get; set; }
    }
}
