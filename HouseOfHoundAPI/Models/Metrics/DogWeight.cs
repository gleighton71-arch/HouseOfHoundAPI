using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Metrics
{
    public class DogWeight
    {
        public int Id { get; set; }

        public int DogId { get; set; }

        public decimal WeightKg { get; set; }

        public DateTime RecordedDateUTC { get; set; }

        public string Note { get; set; }

        public DateTime CreatedDateUTC { get; set; }
    }
}