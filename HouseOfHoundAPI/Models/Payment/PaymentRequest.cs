using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Payment
{
    public class PaymentRequest
    {
        public int PaymentRequestId { get; set; }
        public int InvoiceId { get; set; }
        public decimal Charge { get; set; }
        

        public string StripeSessionId { get; set; }
        public string Status { get; set; }  // Pending, Paid, Failed
        public DateTime CreatedUtc { get; set; }
    }
}