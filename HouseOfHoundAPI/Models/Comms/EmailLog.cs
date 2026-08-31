using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Email
{
    public class EmailLog
    {
        public int EmailLogId { get; set; }
        public int OwnerId { get; set; }
        public string Subject { get; set; }
        public DateTime SentUtc { get; set; }
        public bool Success { get; set; }
    }
}