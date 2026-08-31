using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Forms
{
    public class SubmitOwnerFormDto
    {
        public int OwnerId { get; set; }
        public int TemplateId { get; set; }
        public string JsonData { get; set; }
    }
}