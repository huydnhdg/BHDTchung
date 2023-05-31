using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebBHDTCHUNG.Startup))]
namespace WebBHDTCHUNG
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
