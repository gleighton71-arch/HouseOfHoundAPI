using System;

namespace HouseOfHoundAPI.Controllers.Dog
{
    public class DogSessionHistoryDto
    {
        public int SessionId { get; set; }
        public DateTime? SessionDateUtc { get; set; }
        public string ClinicalNotes { get; set; }
        public string TherapistName { get; set; }
    }
}