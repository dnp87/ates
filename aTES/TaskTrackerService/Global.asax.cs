using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TaskTrackerService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        // dirty hack to create bg thread in order to consume kafka messages
        static readonly Lazy<object> workerFactory =
            new Lazy<object>(() => { StartThread(); return null; });

        private static void StartThread()
        {
            var thread = new Thread(new ThreadStart(new TopicConsumer().ConsumeTopics));
            thread.Start();
        }

        protected void Application_Start()
        {
            var dummy = workerFactory.Value;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
        }
    }
}
