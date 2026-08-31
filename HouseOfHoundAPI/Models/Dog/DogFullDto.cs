using HouseOfHoundAPI.Models.Owner;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dog
{
    public class DogFullDto
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? WeightKg { get; set; }

        public string ImageURL { get; set; }

        public string MicroChip { get; set; }
        public bool IsVetReferral { get; set; }
        public bool IsArchived { get; set; }

        public List<Note> Notes { get; set; } = new List<Note>();

        public string Age
        {
            get
            {
                if (DateOfBirth.HasValue)
                {
                    var today = DateTime.Today;
                    var age = today.Year - DateOfBirth.Value.Year;
                    if (DateOfBirth.Value.Date > today.AddYears(-age)) age--;
                    return age.ToString();
                }
                return "N/A";
            }
        }

        public OwnerSummaryDto Owner { get; set; }

        public List<BookingSummaryDto> Bookings { get; set; } = new List<BookingSummaryDto>();
    }

    public class BookingSummaryDto
    {
        public int BookingId { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public int InvoiceId { get; set; }

        public string Service { get; set; }

        public List<SessionSummaryDto> Sessions { get; set; } = new List<SessionSummaryDto>();
    }

    public class SessionSummaryDto
    {
        public int SessionId { get; set; }
        public DateTime? SessionDateUtc { get; set; }
        public string ClinicalNotes { get; set; }
        public string Therapist { get; set; }
    }
}
