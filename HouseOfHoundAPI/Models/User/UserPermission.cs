using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.User
{
    public class UserPermission
    {
        public int UserPermissionId { get; set; }
        public int UserId { get; set; }

        public bool CanManageBookings { get; set; }
        public bool CanManageInvoices { get; set; }
        public bool CanEditTreatmentPlans { get; set; }
        public bool CanManageStock { get; set; }
        public bool CanSendComms { get; set; }
    }
}