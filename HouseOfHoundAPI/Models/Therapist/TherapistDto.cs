using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models
{
    public class TherapistDto
    {
        public int TherapistId { get; set; }

        public string Name { get; set; }

        public string RegistrationNumber { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public string IdentityUserId { get; set; }

        public DateTime CreatedUtc { get; set; }

        public List<string> Specialities { get; set; }
    }
}