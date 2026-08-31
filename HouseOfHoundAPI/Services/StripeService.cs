using HouseOfHoundAPI.Models.Helper;
using HouseOfHoundAPI.Models.Payment;
using Newtonsoft.Json.Linq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class StripeService
    {

      
        public StripeService()
        {
            
        }

      

        private string GetApiKey()
        {
            var secretKey = AppSettingsService.GetRequiredValue("Stripe_SecretKey");
            var publicKey = AppSettingsService.GetRequiredValue("Stripe_PublicKey");
            var combinedKey = secretKey + ":" + publicKey;
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(combinedKey));
        }

        public async Task<StripeCheckoutSessionResult> CreateCheckoutSessionAsync(
              int invoiceId,
              decimal amount,
              string description)
        {
            string _successUrl = ConfigurationManager.AppSettings["Stripe_SuccessUrl"];
            string _cancelUrl = ConfigurationManager.AppSettings["Stripe_CancelUrl"];
            using (var client = new StripeHttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("mode", "payment"),
                    new KeyValuePair<string,string>("success_url", _successUrl + "?invoiceId=" + invoiceId),
                    new KeyValuePair<string,string>("cancel_url", _cancelUrl + "?invoiceId=" + invoiceId),

                    new KeyValuePair<string,string>("line_items[0][quantity]", "1"),
                    new KeyValuePair<string,string>("line_items[0][price_data][currency]", "gbp"),
                    new KeyValuePair<string,string>("line_items[0][price_data][unit_amount]", ((int)(amount * 100)).ToString()),
                    new KeyValuePair<string,string>("line_items[0][price_data][product_data][name]", description),

                    new KeyValuePair<string,string>("metadata[invoice_id]", invoiceId.ToString())
                });

                var response = await client.PostAsync(HohHelper.GetStripeURL("checkout/sessions"), content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Stripe error: " + json);

                var obj = JObject.Parse(json);

                return new StripeCheckoutSessionResult
                {
                    Id = (string)obj["id"],
                    Url = (string)obj["url"],
                    Status = (string)obj["status"]
                };
            }
        }

      


        public async Task<string> CreatePaymentLink(decimal amount, string description)
        {
            using (var client = new StripeHttpClient())
            {

                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string,string>("line_items[0][price_data][currency]", "gbp"),
            new KeyValuePair<string,string>("line_items[0][price_data][unit_amount]", ((int)(amount * 100)).ToString()),
            new KeyValuePair<string,string>("line_items[0][price_data][product_data][name]", description),
            new KeyValuePair<string,string>("line_items[0][quantity]", "1")
        });

                var response = await client.PostAsync(
                    "/payment_links",
                    content);

                var json = await response.Content.ReadAsStringAsync();

                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                return result.url;
            }
        }

        public async Task<bool> ProcessPayment(PaymentRequest request)
        {
            using (var client = new StripeHttpClient())
            {

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string,string>("amount", (request.Charge * 100).ToString()), // Stripe uses pence
                new KeyValuePair<string,string>("currency", "gbp"),
                new KeyValuePair<string,string>("payment_method_types[]", "card"),
                new KeyValuePair<string,string>("description", "Payment for session " + request.StripeSessionId),
                new KeyValuePair<string,string>("confirm", "true"),
                new KeyValuePair<string,string>("payment_method", "pm_card_visa") // test card
            });

                var response = await client.PostAsync("v1/payment_intents", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Stripe error: " + responseBody);
                }

                Console.WriteLine(responseBody);

                return true;
            }
        }
    }
}
