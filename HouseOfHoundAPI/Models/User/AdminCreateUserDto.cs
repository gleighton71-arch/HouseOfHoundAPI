using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.User
{
    public class AdminCreateUserDto
    {
        public string Email { get; set; }
        public string TemporaryPassword { get; set; }
        public string Role { get; set; }
    }
}