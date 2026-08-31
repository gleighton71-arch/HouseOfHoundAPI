using HouseOfHoundAPI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers
{
    public class UserResetController : ApiController
    {
        //public async Task<IHttpActionResult> GetAsync()
        //{
        //    using (var context = new ApplicationDbContext())
        //    {
        //        var userManager = new UserManager<ApplicationUser>(
        //            new UserStore<ApplicationUser>(context));

        //        var user = await userManager.FindByNameAsync("gleighton71@googlemail.com");

        //        // Or by email:
        //        // var user = await userManager.FindByEmailAsync("you@example.com");

        //        if (user == null)
        //            throw new Exception("User not found");

        //        var removeResult = await userManager.RemovePasswordAsync(user.Id);

        //        if (!removeResult.Succeeded)
        //        {
        //            throw new Exception(string.Join(", ", removeResult.Errors));
        //        }

        //        var addResult = await userManager.AddPasswordAsync(user.Id, "Password123!");

        //        if (!addResult.Succeeded)
        //        {
        //            throw new Exception(string.Join(", ", addResult.Errors));
        //        }
        //    }
        //    return Ok();
        //}
    }
    
}
