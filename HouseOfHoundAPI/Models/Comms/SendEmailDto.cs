using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Comms
{
    public class SendEmailDto
    {
        public int OwnerId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}