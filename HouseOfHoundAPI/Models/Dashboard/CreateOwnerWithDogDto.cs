using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dashboard
{
    public class NewOwnerWithDogRequest
    {
        // Owner details
        public string OwnerFullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Dog details
        public string DogName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Breed { get; set; }
        public string Notes { get; set; }
    }
}