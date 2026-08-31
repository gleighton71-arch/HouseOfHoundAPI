using System;
using System.Collections.Generic;

namespace HouseOfHound.Api.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public int? CustomerId { get; set; }

        public int? DogId { get; set; }

        public DateTime SaleDateUTC { get; set; }

        public string PaymentMethod { get; set; }

        public decimal TotalAmount { get; set; }

        public string ReceiptPdfPath { get; set; }

        public DateTime CreatedDateUTC { get; set; }

        public List<SaleLine> Lines { get; set; }

        public Sale()
        {
            Lines = new List<SaleLine>();
        }
    }
}
