using HouseOfHoundAPI.Models.Dog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Owner
{
    public class OwnerDetailDto
    {
        public int OwnerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public List<DogSummaryDto> Dogs { get; set; }
    }
}