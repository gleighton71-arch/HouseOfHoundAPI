namespace HouseOfHoundAPI.Models
{
    public class TherapistListItemDto
    {
        public int TherapistId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public string RegistrationNumber { get; set; }
    }
}