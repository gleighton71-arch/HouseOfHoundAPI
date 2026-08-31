using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HouseOfHoundAPI.Startup))]
namespace HouseOfHoundAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            AuthStartup.ConfigureAuth(app);
          
        }
    }
}
