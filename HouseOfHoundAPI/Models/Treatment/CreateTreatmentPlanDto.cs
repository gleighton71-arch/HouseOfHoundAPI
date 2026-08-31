using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class CreateTreatmentPlanDto
    {
        public int DogId { get; set; }
        public string Diagnosis { get; set; }
        public string PlanNotes { get; set; }
    }
}