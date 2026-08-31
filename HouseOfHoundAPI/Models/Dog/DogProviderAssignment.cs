using System;

namespace HouseOfHoundAPI.Models.Dog
{
    public class DogProviderAssignment
    {
        public int Id { get; set; }
        public int DogId { get; set; }
        public int? VetRecordId { get; set; }
        public string VetName { get; set; }
        public int? SpecialistProviderRecordId { get; set; }
        public string SpecialistProviderName { get; set; }
        public int? InsurerRecordId { get; set; }
        public string InsurerName { get; set; }
        public int? InsurerPolicyRecordId { get; set; }
        public string PolicyName { get; set; }
        public DateTime AssignedFromUtc { get; set; }
        public DateTime? AssignedToUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
