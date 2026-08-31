using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.TreatmentObject
{
    public class TreatmentObject
    {
        public int TreatmentObjectId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? LastServiceDate { get; set; }

        public DateTime? NextServiceDue { get; set; }

        /// <summary>
        /// Status (e.g. 'A' = Active, 'I' = Inactive)
        /// </summary>
        public char Status { get; set; }
    }
}
