using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Therapist
{
    public class TherapistDiaryEntryDto
    {
        public int BookingId { get; set; }
        public string DogName { get; set; }
        public string OwnerName { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Status { get; set; }
    }
}