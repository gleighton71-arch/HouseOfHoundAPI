using System;

namespace HouseOfHoundAPI.Controllers.Booking
{
    public partial class BookingsController
    {
        public class AvailableSlotRequest
        {
            public DateTime Day { get; set; }
            public int TherapistId { get; set; }
            public int DogId { get; set; }
            public int DurationMinutes { get; set; }
            public int IntervalMinutes { get; set; }
        }
    }
}
