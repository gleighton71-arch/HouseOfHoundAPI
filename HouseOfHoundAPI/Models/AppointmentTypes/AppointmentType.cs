using System;

namespace HouseOfHoundAPI.Models.AppointmentTypes
{
    public class AppointmentType
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
