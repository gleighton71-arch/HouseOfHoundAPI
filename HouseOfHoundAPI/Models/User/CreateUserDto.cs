using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.User
{
    public class CreateUserDto
    {
        public string Email { get; set; }
        public string TemporaryPassword { get; set; }
        public string Role { get; set; }
        public bool MustChangePassword { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public bool MustChangePassword { get; set; }
    }

    public class ResetUserPasswordDto
    {
        public string NewPassword { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
