using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Payments
{
  

    [AllowAnonymous]
    [RoutePrefix("api/stripe")]
    public class StripeWebhookController : ApiController
    {
        private readonly InvoiceRepository _invoiceRepository =
            new InvoiceRepository();

        [HttpPost]
        [Route("webhook")]
        public async Task<IHttpActionResult> Webhook()
        {
            var json = await Request.Content.ReadAsStringAsync();
            var webhookSecret = AppSettingsService.GetValue("Stripe_WebhookSecret");
            if (string.IsNullOrWhiteSpace(webhookSecret))
                return InternalServerError(new InvalidOperationException("Stripe webhook secret is not configured."));

            var signature = Request.Headers.Contains("Stripe-Signature")
                ? Request.Headers.GetValues("Stripe-Signature").FirstOrDefault()
                : null;
            if (string.IsNullOrWhiteSpace(signature)) return Unauthorized();

            if (!IsValidStripeSignature(json, signature, webhookSecret)) return Unauthorized();

            var evt = JObject.Parse(json);

            var eventType = (string)evt["type"];

            if (eventType == "checkout.session.completed")
            {
                var session = evt["data"]?["object"];
                var metadataInvoiceId = (string)session?["metadata"]?["invoice_id"];
                var checkoutSessionId = (string)session?["id"];

                int invoiceId;
                if (int.TryParse(metadataInvoiceId, out invoiceId))
                {
                    _invoiceRepository.MarkPaid(invoiceId,checkoutSessionId);
                }
            }

            return Ok();
        }

        private static bool IsValidStripeSignature(string payload, string signatureHeader, string webhookSecret)
        {
            var parts = signatureHeader
                .Split(',')
                .Select(part => part.Split(new[] { '=' }, 2))
                .Where(part => part.Length == 2)
                .GroupBy(part => part[0], part => part[1])
                .ToDictionary(group => group.Key, group => group.ToArray());

            if (!parts.ContainsKey("t") || !parts.ContainsKey("v1")) return false;
            long timestamp;
            if (!long.TryParse(parts["t"].FirstOrDefault(), out timestamp)) return false;

            var eventTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddSeconds(timestamp);
            if (Math.Abs((DateTimeOffset.UtcNow - eventTime).TotalMinutes) > 5) return false;

            var signedPayload = timestamp + "." + payload;
            var expectedSignature = ComputeHmacSha256(signedPayload, webhookSecret);

            return parts["v1"].Any(signature => FixedTimeEquals(expectedSignature, signature));
        }

        private static string ComputeHmacSha256(string value, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private static bool FixedTimeEquals(string expected, string actual)
        {
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual)) return false;

            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var actualBytes = Encoding.UTF8.GetBytes(actual.ToLowerInvariant());
            var diff = expectedBytes.Length ^ actualBytes.Length;
            var length = Math.Min(expectedBytes.Length, actualBytes.Length);

            for (var i = 0; i < length; i++)
            {
                diff |= expectedBytes[i] ^ actualBytes[i];
            }

            return diff == 0;
        }
    }
}
