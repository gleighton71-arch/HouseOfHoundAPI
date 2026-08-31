using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Forms
{
    public class OwnerFormTemplate
    {
        public int OwnerFormTemplateId { get; set; }
        public string Name { get; set; }
        public string JsonSchema { get; set; } // dynamic form rendering
    }
}