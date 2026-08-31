using System;

namespace HouseOfHoundAPI.Models.Vets
{
    public class Vet
    {
        public int Id { get; set; }
        public string VetId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactName { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
