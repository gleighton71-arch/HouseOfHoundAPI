using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dog
{
    public class DogSummaryDto
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public DateTime? DOB { get; set; }
    }
}