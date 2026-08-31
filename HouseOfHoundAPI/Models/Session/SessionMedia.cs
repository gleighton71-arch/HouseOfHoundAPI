using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Session
{
    public class SessionMedia
    {
        public int SessionMediaId { get; set; }
        public int SessionId { get; set; }

        public string MediaType { get; set; }  // Image, Video
        public string StoragePath { get; set; }
        public DateTime UploadedUtc { get; set; }
    }
}