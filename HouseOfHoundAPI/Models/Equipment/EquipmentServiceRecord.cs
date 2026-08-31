using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Equipment
{
    public class EquipmentServiceRecord
    {
        public int EquipmentServiceRecordId { get; set; }
        public int EquipmentId { get; set; }
        public DateTime ServiceDateUtc { get; set; }
        public string Notes { get; set; }
    }
}