namespace HouseOfHound.Api.Models
{
    public class SaleLine
    {
        public int Id { get; set; }

        public int SaleId { get; set; }

        public int StockItemId { get; set; }

        public string StockCode { get; set; }

        public string StockDescription { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal? UnitCost { get; set; }

        public decimal LineTotal { get; set; }
    }
}