using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Equipment
{
    public class CreateEquipmentServiceDto
    {
        public int EquipmentId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceInterval { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string Status { get; set; }
        public DateTime ServiceDueDate { get; set; }
        public DateTime? BookedServiceDate { get; set; }
        public string Notes { get; set; }
    }
}
