using System;

namespace HouseOfHoundAPI.Models.Session
{
    public class SessionWorkItemDto
    {
        public int BookingId { get; set; }
        public int DogId { get; set; }
        public int OwnerId { get; set; }
        public string DogName { get; set; }
        public string Breed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ImageURL { get; set; }
        public int TherapistId { get; set; }
        public string TherapistName { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public decimal? Cost { get; set; }
        public int? AppointmentTypeId { get; set; }
        public string AppointmentTypeCode { get; set; }
        public string AppointmentTypeDescription { get; set; }
        public int? InvoiceId { get; set; }
        public string InvoiceStatus { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
        public bool IsPaid { get; set; }
    }

    public class UpdateSessionBookingStatusRequest
    {
        public string Status { get; set; }
    }
}
