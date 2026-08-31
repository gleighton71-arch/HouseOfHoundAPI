using System;
namespace HouseOfHoundAPI.Models.User
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public bool MustChangePassword { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
    }
}
