using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentAction
    {
        public string Description { get; set; }

        public string Duration { get; set; } // keeping string to match "5 mins"
    }
}