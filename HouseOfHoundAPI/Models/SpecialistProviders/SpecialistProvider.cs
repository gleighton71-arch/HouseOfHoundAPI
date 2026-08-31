using System;

namespace HouseOfHoundAPI.Models.SpecialistProviders
{
    public class SpecialistProvider
    {
        public int Id { get; set; }
        public string SpecialistId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
