using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(excelwebtest2.Startup))]
namespace excelwebtest2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
