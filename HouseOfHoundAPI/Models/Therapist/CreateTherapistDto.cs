using System.Collections.Generic;

namespace HouseOfHoundAPI.Models
{
    public class CreateTherapistDto
    {
        public string Name { get; set; }

        public string RegistrationNumber { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public string IdentityUserId { get; set; }

        public List<string> Specialities { get; set; }
    }
}