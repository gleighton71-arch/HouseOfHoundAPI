using System;

namespace HouseOfHound.Api.Models
{
    public class StockItem
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public int QuantityInStock { get; set; }

        public int MinimumStockHolding { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SalePrice { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDateUTC { get; set; }

        public DateTime? UpdatedDateUTC { get; set; }
    }
}
