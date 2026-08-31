using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlan
    {
        public string PlanName { get; set; }

        public string PlanDescription { get; set; }

        public string Interval { get; set; } // e.g. Weekly, Daily

        public int SessionCount { get; set; }

        public decimal CostPerSession { get; set; }

        public List<TreatmentService> Services { get; set; } = new List<TreatmentService>();
    }
}