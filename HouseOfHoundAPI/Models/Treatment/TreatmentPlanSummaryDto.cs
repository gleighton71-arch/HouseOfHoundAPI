using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentPlanSummaryDto
    {
        public int TreatmentPlanId { get; set; }
        public int DogId { get; set; }
        public bool IsActive { get; set; }
    }
}