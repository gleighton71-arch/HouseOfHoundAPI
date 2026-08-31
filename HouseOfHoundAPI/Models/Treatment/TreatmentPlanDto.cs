using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlanDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public string Interval { get; set; }
        public int SessionCount { get; set; }
        public decimal? CostPerSession { get; set; }

        public List<TreatmentServiceDto> Services { get; set; } = new List<TreatmentServiceDto>();
    }
}