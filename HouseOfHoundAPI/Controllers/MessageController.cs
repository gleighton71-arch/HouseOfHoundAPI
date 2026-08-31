using HouseOfHoundAPI.Models.Comms;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HouseOfHoundAPI.Controllers
{
    //[Authorize]
    public class MessageController : ApiController
    {
        private string _fromPhoneNumber = AppSettingsService.GetRequiredValue("Twilio_WhatsAppFrom");


        [HttpPost]
        [Route("api/message/send-email")]
        public async Task<IHttpActionResult> SendEmail(SendEmailDto sendEmailDto)
        {
            EmailService emailService = new EmailService();
            OwnerService ownerService = new OwnerService();
            if (sendEmailDto == null)
            {
                return BadRequest("Invalid request data.");
            }


            Models.Owner.Owner owner = ownerService.GetOwner(sendEmailDto.OwnerId);
            if (owner == null)
            {
                return BadRequest("Invalid request data. Owner not found.");
            }


            var defaultEmail = ConfigurationManager.AppSettings["DefaultEmail"];
            if (!string.IsNullOrEmpty(defaultEmail))
            {
                owner.Email = defaultEmail;
            }

            

            await  emailService.SendEmail(owner.Email, sendEmailDto.Subject,  sendEmailDto.Body);

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] Models.Messaging.MessageRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("PhoneNumber and Message are required.");
            }
            try
            {
                // Here you would integrate with your SMS service provider to send the message.
                // For example, using Twilio:

                var accountSid = AppSettingsService.GetRequiredValue("TwilioAccountSid");
                var authToken = AppSettingsService.GetRequiredValue("TwilioAuthToken");
                var twilioClient = new TwilioRestClient(accountSid, authToken);
                var message = MessageResource.Create(
                    to: new PhoneNumber(request.PhoneNumber),
                    from: new PhoneNumber(_fromPhoneNumber),
                    body: request.Message,
                    client: twilioClient
                );


           

                // Simulate sending message
                Console.WriteLine($"Sending message to {request.PhoneNumber}: {request.Message}");
                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return InternalServerError(ex);
            }
        }
    }
}
