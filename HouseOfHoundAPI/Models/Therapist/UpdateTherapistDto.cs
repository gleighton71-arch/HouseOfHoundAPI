using System.Collections.Generic;

namespace HouseOfHoundAPI.Models
{
    public class UpdateTherapistDto
    {
        public string Name { get; set; }

        public string RegistrationNumber { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; }

        public string IdentityUserId { get; set; }

        public List<string> Specialities { get; set; }
    }
}