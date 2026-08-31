using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool MustChangePassword { get; set; }
    }
}