using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Session
{
    public class CreateSessionDto
    {
        public int BookingId { get; set; }
        public DateTime? SessionDateUtc { get; set; } // if null => now
        public string ClinicalNotes { get; set; }
    }
}