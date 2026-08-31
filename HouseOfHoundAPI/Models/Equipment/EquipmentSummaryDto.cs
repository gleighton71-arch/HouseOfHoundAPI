using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Equipment
{
    public class EquipmentSummaryDto
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; }
        public bool IsOperational { get; set; }
    }
}