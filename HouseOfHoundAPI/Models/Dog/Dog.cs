using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dog
{
    public class Dog
    {
        public int DogId { get; set; }
        public int OwnerId { get; set; }

        public string Name { get; set; }
        public string Breed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? WeightKg { get; set; }

        public string Age
        {
            get
            {
                if ( DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateOfBirth.Value.Year;
                    if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                    return age.ToString();
                }
                return "N/A";
            }
        }

        public string MicroChipNumber { get; set; } 
        public bool HasAllergies { get; set; }
        public string Notes { get; set; }
        public bool IsArchived { get; set; }
    }
}
