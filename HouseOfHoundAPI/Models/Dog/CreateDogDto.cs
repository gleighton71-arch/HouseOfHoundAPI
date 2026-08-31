using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dog
{
    public class CreateDogDto
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? WeightKg { get; set; }
        public string Notes { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public string MicroChip { get; set; }
        public bool IsVetReferral { get; set; }
        public bool IsArchived { get; set; }
    }
}
