using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Equipment
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public bool HasValue { get; set; }

        public decimal? Value { get; set; }

        public string SerialNumber { get; set; }

        public string Status { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedDate { get; set; }

        public List<EquipmentServiceSchedule> ServiceSchedules { get; set; } = new List<EquipmentServiceSchedule>();
    }

    public class EquipmentServiceSchedule
    {
        public int EquipmentServiceScheduleId { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceInterval { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string Status { get; set; }
        public DateTime ServiceDueDate { get; set; }
        public DateTime? BookedServiceDate { get; set; }
        public string Notes { get; set; }
    }
}
