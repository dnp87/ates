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
        // dirty hack to create bg threads in order to consume kafka messages
        static readonly Lazy<object> parrotCreatedWorkerFactory =
            new Lazy<object>(() => { StartThread(new TopicConsumer().ConsumeParrotCreatedTopic); return null; });
        static readonly Lazy<object> parrotUpdatedWorkerFactory =
            new Lazy<object>(() => { StartThread(new TopicConsumer().ConsumeParrotUpdatedTopic); return null; });

        private static void StartThread(Action action)
        {
            var thread = new Thread(new ThreadStart(action));
            thread.Start();
        }

        protected void Application_Start()
        {
            var dummy1 = parrotCreatedWorkerFactory.Value;
            var dummy2 = parrotUpdatedWorkerFactory.Value;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
        }
    }
}
