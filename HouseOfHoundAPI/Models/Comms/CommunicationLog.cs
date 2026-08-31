using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Comms
{
    public class CommunicationLog
    {
        public int CommunicationLogId { get; set; }
        public int OwnerId { get; set; }

        public string Channel { get; set; } // WhatsApp, SMS
        public string Message { get; set; }
        public DateTime SentUtc { get; set; }
        public bool Success { get; set; }
    }
}