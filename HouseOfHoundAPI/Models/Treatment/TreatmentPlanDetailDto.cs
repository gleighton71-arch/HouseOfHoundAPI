using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlanDetailDto
    {
        public int TreatmentPlanId { get; set; }
        public string Diagnosis { get; set; }
        public string PlanNotes { get; set; }

        public List<TreatmentPlanAreaDto> Areas { get; set; }
    }
}