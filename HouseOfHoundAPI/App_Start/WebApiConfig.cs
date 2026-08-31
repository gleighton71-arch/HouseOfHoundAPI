using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using System.Web.Http.Cors;

namespace HouseOfHoundAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            var cors = new EnableCorsAttribute(
                  "http://127.0.0.1:5500",
                  "*",
                  "*");

            config.EnableCors(cors);

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            // Web API configuration and services
            config.Filters.Add(new AuthorizeAttribute());
         
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
