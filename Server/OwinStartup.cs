using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Server.OwinStartup))]

namespace Server
{
    public class OwinStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            // Map SignalR hubs
            appBuilder.MapSignalR();
        }
    }
}