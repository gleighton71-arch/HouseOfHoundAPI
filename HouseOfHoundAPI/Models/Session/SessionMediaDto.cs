using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Session
{
    public class SessionMediaDto
    {
        public int SessionMediaId { get; set; }
        public string MediaType { get; set; }
        public string Url { get; set; }
    }
}