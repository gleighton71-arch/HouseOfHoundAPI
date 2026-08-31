using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Messaging
{
    public class MessageRequest
    {
        public string PhoneNumber { get; set; } 
        public string Message { get; set; }
    }
}