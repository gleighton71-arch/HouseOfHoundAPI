using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace HouseOfHoundAPI.Models.Payment
{
    public class StripeHttpClient : HttpClient
    {
       

        public StripeHttpClient()
        {
            this.BaseAddress = new Uri(ConfigurationManager.AppSettings["Stripe_URL_Base"]);
            this.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", GetApiKey());
        }

        private string GetApiKey()
        {
            var secretKey = AppSettingsService.GetRequiredValue("Stripe_SecretKey");
            return secretKey;
        }

      




    }
}
