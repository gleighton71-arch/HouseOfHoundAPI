using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentArea
    {
        public int TreatmentAreaId { get; set; }
        public string Code { get; set; }     // rear-left-leg
        public string DisplayName { get; set; }
    }
}