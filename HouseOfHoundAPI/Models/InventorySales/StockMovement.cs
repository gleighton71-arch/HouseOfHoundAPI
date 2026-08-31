using System;

namespace HouseOfHound.Api.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int StockItemId { get; set; }

        public string StockCode { get; set; }

        public string StockDescription { get; set; }

        public string MovementType { get; set; }

        public int QuantityChange { get; set; }

        public string ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public string Note { get; set; }

        public DateTime CreatedDateUTC { get; set; }
    }
}