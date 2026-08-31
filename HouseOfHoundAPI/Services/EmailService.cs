using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace HouseOfHoundAPI.Services
{
    public class EmailService
    {

        public async Task SendEmail(string email,string subject, string body)
        {

            var defaultEmail = ConfigurationManager.AppSettings["DefaultEmail"];

            if (!string.IsNullOrEmpty(defaultEmail))
            {
                email = defaultEmail;
            }

            body = body.Replace("\n", "<BR>");

            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var user = ConfigurationManager.AppSettings["SmtpUser"];
            var pass = ConfigurationManager.AppSettings["SmtpPass"];
            var port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            port = 587;
            var from = ConfigurationManager.AppSettings["SmtpFrom"];
            using (var client = new SmtpClient(host,port))
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(user, pass);
                var mail = new MailMessage(
                    from,
                    email,
                    subject,
                    body);
                mail.IsBodyHtml = true;
                await client.SendMailAsync(mail);
            }
        }

        public async Task SendPaymentLinkAsync(string email, string name, decimal amount, string paymentUrl)
        {
            var body = $@"
Hello {name},

Please complete your payment of £{amount} using the secure link below.

{paymentUrl}

Thank you,
House Of Hound
";
            var host = ConfigurationManager.AppSettings["SmtpHost"];
            var user = ConfigurationManager.AppSettings["SmtpUser"];
            var pass = ConfigurationManager.AppSettings["SmtpPass"];
            var port = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            port = 587;
            var from = ConfigurationManager.AppSettings["SmtpFrom"];
            using (var client = new SmtpClient(host,port))
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(user, pass);

                var mail = new MailMessage(
                    from,
                    email,
                    "Payment for your hydrotherapy session",
                    body);

                await client.SendMailAsync(mail);
            }
        }
    }
}