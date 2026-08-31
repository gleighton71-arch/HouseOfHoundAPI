using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlanAreaDto
    {
        public int TreatmentAreaId { get; set; }
        public string AreaCode { get; set; }
        public string DisplayName { get; set; }
        public string Condition { get; set; }
        public int Severity { get; set; }
    }
}