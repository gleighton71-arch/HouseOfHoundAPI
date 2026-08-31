using HouseOfHoundAPI.Models.Settings;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [RoutePrefix("api/settings")]
    public class SettingsController : ApiController
    {
        private static readonly HashSet<string> AllowedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TwilioAccountSid",
            "TwilioAuthToken",
            "Twilio_WhatsAppFrom",
            "Stripe_PublicKey",
            "Stripe_SecretKey",
            "Stripe_WebhookSecret"
        };

        [HttpPut, Route("{key}")]
        public IHttpActionResult Save(string key, ApplicationSettingDto dto)
        {
            if (string.IsNullOrWhiteSpace(key)) return BadRequest("Setting key required.");
            if (!AllowedKeys.Contains(key)) return BadRequest("Setting key is not allowed.");
            if (dto == null || string.IsNullOrWhiteSpace(dto.Value)) return BadRequest("Setting value required.");

            if (dto.IsSecret)
            {
                AppSettingsService.SaveSecret(key, dto.Value);
            }
            else
            {
                AppSettingsService.SavePlainText(key, dto.Value);
            }

            return Ok(new
            {
                Key = key,
                IsSecret = dto.IsSecret,
                Saved = true
            });
        }
    }
}
