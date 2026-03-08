using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace KumariCinema
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            try
            {
                // Register routes
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                
                // Register bundles
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                
                System.Diagnostics.Debug.WriteLine("✅ Application started successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Application start error: {ex.Message}");
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            System.Diagnostics.Debug.WriteLine($"❌ Application error: {ex?.Message}");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Initialize session if needed
        }
    }
}