using NLog;
using System;
using System.Web;
using System.Web.Http;

namespace Server
{
    public class WebApiApplication : HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            UnityConfig.RegisterComponents();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_Error(object sender, EventArgs evetArgs)
        {
            Exception exception = Server.GetLastError();
            Logger.Error(exception, "Unhandled exception occurred.");
        }
    }
}
