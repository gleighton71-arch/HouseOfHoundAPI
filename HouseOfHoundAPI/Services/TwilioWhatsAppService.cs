using HouseOfHoundAPI.Models.Payment;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Types;

using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class TwilioWhatsAppService
{

    private readonly string _fromPhoneNumber = AppSettingsService.GetRequiredValue("Twilio_WhatsAppFrom");
    // e.g. whatsapp:+14155238886 for sandbox or your production sender

    public TwilioWhatsAppResult SendPaymentLinkAsync(
        string toPhoneNumber,
        string customerName,
        decimal amount,
        string paymentUrl)
    {


        var accountSid = AppSettingsService.GetRequiredValue("TwilioAccountSid");
        var authToken = AppSettingsService.GetRequiredValue("TwilioAuthToken");
        var twilioClient = new TwilioRestClient(accountSid, authToken);
        var message = MessageResource.Create(
            to: new PhoneNumber(toPhoneNumber),
            from: new PhoneNumber(_fromPhoneNumber),
            body: "Hi " + customerName + ",\n\n" +
                "Your House of Hound payment request is ready.\n" +
                "Amount: £" + amount.ToString("0.00") + "\n\n" +
                "Please pay securely here:\n" +
                paymentUrl + "\n\n" +
                "Thank you.",
            client: twilioClient
        );

        return new TwilioWhatsAppResult
        {
            Sid = message.Sid,
            Status = message.Status?.ToString()
        };
    }
}

