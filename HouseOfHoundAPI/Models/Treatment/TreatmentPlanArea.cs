using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlanArea
    {
        public int TreatmentPlanAreaId { get; set; }
        public int TreatmentPlanId { get; set; }
        public int TreatmentAreaId { get; set; }

        public string Condition { get; set; }  // muscle soreness
        public int Severity { get; set; }      // 1-5
    }
}