using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Niawa.WebNotify.TestWebClient3.Startup))]

namespace Niawa.WebNotify.TestWebClient3
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}
