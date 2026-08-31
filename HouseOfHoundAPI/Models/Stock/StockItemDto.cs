using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Stock
{
    public class StockItemDto
    {
        public int StockItemId { get; set; }
        public string Name { get; set; }
        public int QuantityOnHand { get; set; }
    }
}