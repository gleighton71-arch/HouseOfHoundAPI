using System;
using System.Collections.Generic;

namespace HouseOfHoundAPI.Models.Insurers
{
    public class Insurer
    {
        public int Id { get; set; }
        public string InsurerId { get; set; }
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
        public List<InsurerPolicy> Policies { get; set; } = new List<InsurerPolicy>();
    }
}
