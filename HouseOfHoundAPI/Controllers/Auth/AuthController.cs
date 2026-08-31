using HouseOfHoundAPI;
using HouseOfHoundAPI.Models.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using HouseOfHoundAPI.Models;


namespace HouseOfHoundAPI.Controllers.Booking
{

    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly JwtTokenService _jwt = new JwtTokenService();


        private ApplicationUserManager UserManager =>
            HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
    

        [HttpPost, Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login(LoginDto dto)
        {
            if (dto == null) return BadRequest("Body required.");
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password required.");

            var user = await UserManager.FindByNameAsync(dto.Username);
            if (user == null)
                return Unauthorized();

            var ok = await UserManager.CheckPasswordAsync(user, dto.Password);
            if (!ok)
                return Unauthorized();

            // Pull roles (Identity roles)
            var roles = (await UserManager.GetRolesAsync(user.Id)).ToArray();

            // If you extended ApplicationUser with MustChangePassword (recommended)
            var mustChange = (user is HouseOfHoundAPI.Models.ApplicationUser au) && au.MustChangePassword;

            var expiryHours = int.Parse(ConfigurationManager.AppSettings["JwtExpiryHours"]);
            var expiresUtc = DateTime.UtcNow.AddHours(expiryHours);

            var token = _jwt.CreateToken(
                userId: user.Id,
                email: user.Email,
                roles: roles,
                mustChangePassword: mustChange
            );

            return Ok(new LoginResponseDto
            {
                Token = token,
                ExpiresUtc = expiresUtc,
                MustChangePassword = mustChange,
                Username = user.UserName,
                Email = user.Email,
                Roles = roles
            });
        }

        [HttpGet, Route("me")]
        [Authorize]
        public async Task<IHttpActionResult> Me()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            return Ok(new
            {
                Username = user.UserName,
                Email = user.Email,
                Roles = (await UserManager.GetRolesAsync(user.Id)).ToArray()
            });
        }

        [HttpPost, Route("change-password")]
        [Authorize]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDto dto)
        {
            if (dto == null) return BadRequest("Body required.");
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest("CurrentPassword and NewPassword required.");

            var userId = User.Identity.GetUserId();
            var result = await UserManager.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(string.Join("; ", result.Errors));

            // Clear MustChangePassword flag if you’re using it
            var user = await UserManager.FindByIdAsync(userId);
            if (user is HouseOfHoundAPI.Models.ApplicationUser au)
            {
                au.MustChangePassword = false;
                await UserManager.UpdateAsync(user);
            }

            return Ok();
        }
    }
}
