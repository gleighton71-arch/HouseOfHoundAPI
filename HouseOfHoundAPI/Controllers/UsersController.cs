using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Models.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers
{
    //[Authorize(Roles = "Admin")]
    [RoutePrefix("api/users")]
    public class UsersController : ApiController
    {
        private ApplicationUserManager UserManager =>
            HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var identityUsers = UserManager.Users.OrderBy(u => u.Email).ToList();
            var users = new List<object>();

            foreach (var user in identityUsers)
            {
                users.Add(await BuildUserResult(user.Id));
            }

            return Ok(users);
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Create(CreateUserDto dto)
        {
            var validationError = ValidateUserPayload(dto?.Email, dto?.Role, dto?.TemporaryPassword);
            if (validationError != null) return validationError;

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                MustChangePassword = dto.MustChangePassword
            };

            var result = await UserManager.CreateAsync(user, dto.TemporaryPassword);
            if (!result.Succeeded) return BadRequest(string.Join("; ", result.Errors));

            var roleResult = await SetUserRole(user.Id, dto.Role);
            if (roleResult != null) return roleResult;

            return Ok(await BuildUserResult(user.Id));
        }

        [HttpPut, Route("{id}")]
        public async Task<IHttpActionResult> Update(string id, UpdateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("User id required.");
            if (dto == null) return BadRequest("Body required.");
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email required.");
            if (string.IsNullOrWhiteSpace(dto.Role)) return BadRequest("Role required.");

            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.UserName = dto.Email;
            user.Email = dto.Email;
            user.MustChangePassword = dto.MustChangePassword;

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(string.Join("; ", result.Errors));

            var roleResult = await SetUserRole(user.Id, dto.Role);
            if (roleResult != null) return roleResult;

            return Ok(await BuildUserResult(user.Id));
        }

        [HttpPost, Route("{id}/reset-password")]
        public async Task<IHttpActionResult> ResetPassword(string id, ResetUserPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("User id required.");
            if (dto == null || string.IsNullOrWhiteSpace(dto.NewPassword)) return BadRequest("New password required.");

            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var removeResult = await UserManager.RemovePasswordAsync(user.Id);
            if (!removeResult.Succeeded) return BadRequest(string.Join("; ", removeResult.Errors));

            var addResult = await UserManager.AddPasswordAsync(user.Id, dto.NewPassword);
            if (!addResult.Succeeded) return BadRequest(string.Join("; ", addResult.Errors));

            user.MustChangePassword = dto.MustChangePassword;
            var updateResult = await UserManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest(string.Join("; ", updateResult.Errors));

            return Ok(await BuildUserResult(user.Id));
        }

        [HttpPost, Route("{id}/disable")]
        public async Task<IHttpActionResult> Disable(string id)
        {
            return await SetDisabled(id, true);
        }

        [HttpPost, Route("{id}/enable")]
        public async Task<IHttpActionResult> Enable(string id)
        {
            return await SetDisabled(id, false);
        }

        private IHttpActionResult ValidateUserPayload(string email, string role, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email required.");
            if (string.IsNullOrWhiteSpace(role)) return BadRequest("Role required.");
            if (string.IsNullOrWhiteSpace(password)) return BadRequest("Temporary password required.");
            return null;
        }

        private async Task<IHttpActionResult> SetUserRole(string userId, string role)
        {
            var roleResult = await EnsureRole(role);
            if (roleResult != null) return roleResult;

            var currentRoles = await UserManager.GetRolesAsync(userId);
            if (currentRoles.Any())
            {
                var removeResult = await UserManager.RemoveFromRolesAsync(userId, currentRoles.ToArray());
                if (!removeResult.Succeeded) return BadRequest(string.Join("; ", removeResult.Errors));
            }

            var addResult = await UserManager.AddToRoleAsync(userId, role);
            if (!addResult.Succeeded) return BadRequest(string.Join("; ", addResult.Errors));

            return null;
        }

        private async Task<IHttpActionResult> EnsureRole(string role)
        {
            using (var context = new ApplicationDbContext())
            using (var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context)))
            {
                if (await roleManager.RoleExistsAsync(role)) return null;

                var result = await roleManager.CreateAsync(new IdentityRole(role));
                return result.Succeeded ? null : BadRequest(string.Join("; ", result.Errors));
            }
        }

        private async Task<IHttpActionResult> SetDisabled(string id, bool disabled)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("User id required.");

            var user = await UserManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var lockoutEnabledResult = await UserManager.SetLockoutEnabledAsync(user.Id, true);
            if (!lockoutEnabledResult.Succeeded) return BadRequest(string.Join("; ", lockoutEnabledResult.Errors));

            var lockoutEnd = disabled
                ? DateTimeOffset.UtcNow.AddYears(100)
                : DateTimeOffset.UtcNow.AddMinutes(-1);
            var lockoutResult = await UserManager.SetLockoutEndDateAsync(user.Id, lockoutEnd);
            if (!lockoutResult.Succeeded) return BadRequest(string.Join("; ", lockoutResult.Errors));

            return Ok(await BuildUserResult(user.Id));
        }

        private async Task<object> BuildUserResult(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var roles = await UserManager.GetRolesAsync(user.Id);

            return new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.LockoutEnabled,
                IsLockedOut = await UserManager.IsLockedOutAsync(user.Id),
                user.MustChangePassword,
                Role = roles.FirstOrDefault() ?? "",
                Roles = roles.ToArray()
            };
        }
    }
}
