using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Invoice
{
    public class InvoiceSummaryDto
    {
        public DateTime InvoiceDate { get; set; }
        public DateTime? BookingDate { get; set; }
        public int InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int? BookingId { get; set; }
        public int? DogId { get; set; }
        public string DogName { get; set; }
        public string StripeCheckoutUrl { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }
}
