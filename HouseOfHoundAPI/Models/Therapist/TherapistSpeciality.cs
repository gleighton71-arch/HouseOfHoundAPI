using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Therapist
{
    public class TherapistSpeciality
    {
        public int Id { get; set; }

        public int TherapistId { get; set; }

        public string Name { get; set; } // Hydro, Laser, Rehab

        public Therapist Therapist { get; set; }
    }
}