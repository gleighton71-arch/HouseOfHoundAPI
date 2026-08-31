using System;

namespace HouseOfHoundAPI.Models.Insurers
{
    public class InsurerPolicy
    {
        public int Id { get; set; }
        public int InsurerRecordId { get; set; }
        public string PolicyId { get; set; }
        public string Name { get; set; }
        public string BriefDetails { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
