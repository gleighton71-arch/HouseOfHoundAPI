using HouseOfHoundAPI.Models.Treatment;
using System.Collections.Generic;

namespace HouseOfHoundAPI.Services
{
    public class TreatmentService
    {
        public string Name { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Cost { get; set; }

        public List<TreatmentAction> Actions { get; set; } = new List<TreatmentAction>();
    }
}
