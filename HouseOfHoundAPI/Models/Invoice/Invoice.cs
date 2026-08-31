using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Invoice
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int OwnerId { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; }  // Draft, Sent, Paid
        public DateTime CreatedUtc { get; set; }
    }
}