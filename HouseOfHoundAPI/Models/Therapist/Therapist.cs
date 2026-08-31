using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Therapist
{
    public class Therapist
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public string IdentityUserId { get; set; } // optional link to ApplicationUser

        public virtual ICollection<TherapistSpeciality> Specialities { get; set; }
    }
}