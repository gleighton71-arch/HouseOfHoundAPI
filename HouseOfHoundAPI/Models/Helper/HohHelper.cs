using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Helper
{
    public static class HohHelper
    {
        public static string GetStripeURL(string endpoint)
        {
            string urlBase = ConfigurationManager.AppSettings["Stripe_URL_Base"];

            while (urlBase.EndsWith("/"))
            {
                urlBase = urlBase.Substring(0, urlBase.Length - 1);
            }

            while (endpoint.StartsWith("/"))
            {
                endpoint = endpoint.Substring(1);
            }

            return urlBase + "/" + endpoint;
        }

    }
}