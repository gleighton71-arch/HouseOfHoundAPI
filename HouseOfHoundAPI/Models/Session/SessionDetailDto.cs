using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Session
{
    public class SessionDetailDto
    {
        public int SessionId { get; set; }
        public int BookingId { get; set; }
        public DateTime SessionDateUtc { get; set; }
        public string ClinicalNotes { get; set; }

        public List<SessionMediaDto> Media { get; set; }
    }
}