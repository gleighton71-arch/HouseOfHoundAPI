using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Comms
{
    public class SendWhatsAppDto
    {
        public int OwnerId { get; set; }
        public string Message { get; set; }
    }
}