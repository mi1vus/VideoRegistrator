using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using MyHomeSecureWeb.DataObjects;
using MyHomeSecureWeb.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using System.Net.Http.Headers;
using MyHomeSecureWeb.Utilities;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Web.Http.Cors;
using System.Web.Http.Routing;

namespace MyHomeSecureWeb
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            //var container = new UnityContainer();
            //container.RegisterType<IAwayStatusRepository, AwayStatusRepository>(new HierarchicalLifetimeManager());
            //container.RegisterType<ILogRepository, LogRepository>(new HierarchicalLifetimeManager());

            //container.RegisterType<IServiceSettingsProvider, ServiceSettingsProvider>(new HierarchicalLifetimeManager());
            //container.RegisterType<IServiceTokenHandler, ServiceTokenHandler>(new HierarchicalLifetimeManager());

            //config.DependencyResolver = new UnityResolver(container);
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new MobileServiceInitializer());
        }
    }

    public class MobileServiceInitializer : DropCreateDatabaseIfModelChanges<MobileServiceContext>
    {
        protected override void Seed(MobileServiceContext context)
        {
            base.Seed(context);
        }
    }
}

