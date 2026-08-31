using System.Collections.Generic;

namespace HouseOfHoundAPI.Models.Treatment
{
    public class TreatmentServiceDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Cost { get; set; }

        public List<TreatmentActionDto> Actions { get; set; } = new List<TreatmentActionDto>();
    }
}
