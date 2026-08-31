using System.Collections.Generic;

namespace HouseOfHound.Api.Models
{
    public class CreateSaleRequest
    {
        public string CustomerName { get; set; }

        public int? CustomerId { get; set; }

        public int? DogId { get; set; }

        public string PaymentMethod { get; set; }

        public string ReceiptPdfPath { get; set; }

        public List<CreateSaleLineRequest> Lines { get; set; }

        public CreateSaleRequest()
        {
            Lines = new List<CreateSaleLineRequest>();
        }
    }

    public class CreateSaleLineRequest
    {
        public int StockItemId { get; set; }

        public int Quantity { get; set; }
    }
}
