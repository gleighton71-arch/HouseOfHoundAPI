using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Invoice
{
    public class CreateInvoiceDto
    {
        public int OwnerId { get; set; }
        public List<CreateInvoiceLineDto> Lines { get; set; }
    }
}