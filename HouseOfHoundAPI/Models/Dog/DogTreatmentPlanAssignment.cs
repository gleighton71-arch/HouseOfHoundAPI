using System;
using System.Collections.Generic;

namespace HouseOfHoundAPI.Models.Dog
{
    public class DogTreatmentPlanAssignment
    {
        public int Id { get; set; }
        public int DogId { get; set; }
        public int SourceTreatmentPlanId { get; set; }
        public string PlanName { get; set; }
        public string PlanDescription { get; set; }
        public string Interval { get; set; }
        public int SessionCount { get; set; }
        public decimal? CostPerSession { get; set; }
        public DateTime AssignedDateUtc { get; set; }
        public DateTime? CompletedDateUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
        public List<DogTreatmentPlanAssignmentService> Services { get; set; } = new List<DogTreatmentPlanAssignmentService>();
    }

    public class DogTreatmentPlanAssignmentService
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int? SourceServiceId { get; set; }
        public string Name { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Cost { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CreateDogTreatmentPlanAssignmentRequest
    {
        public int TreatmentPlanId { get; set; }
        public DateTime? AssignedDateUtc { get; set; }
    }

    public class CompleteDogTreatmentPlanAssignmentRequest
    {
        public DateTime? CompletedDateUtc { get; set; }
    }
}
