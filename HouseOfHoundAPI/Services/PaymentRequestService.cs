using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Models.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HouseOfHoundAPI.Services
{
    public class PaymentRequestService
    {
        private readonly StripeService _stripeService;
        private readonly TwilioWhatsAppService _twilioService;
        private readonly InvoiceRepository _invoiceRepository;

        public PaymentRequestService(
            StripeService stripeService,
            TwilioWhatsAppService twilioService,
            InvoiceRepository invoiceRepository)
        {
            _stripeService = stripeService;
            _twilioService = twilioService;
            _invoiceRepository = invoiceRepository;
        }

        public async Task SendInvoicePaymentLinkAsync(SendInvoicePaymentLinkRequest request)
        {
            var stripe = await _stripeService.CreateCheckoutSessionAsync(
                request.InvoiceId,
                request.Amount,
                request.Description);

            string twilioSid = null;
            bool whatsappSent = false;
            bool emailSent = false;

            if (!string.IsNullOrWhiteSpace(request.WhatsAppNumber))
            {
                try
                {
                    var twilio = _twilioService.SendPaymentLinkAsync(
                        request.WhatsAppNumber,
                        request.CustomerName,
                        request.Amount,
                        stripe.Url);

                    twilioSid = twilio?.Sid;
                    whatsappSent = true;
                }
                catch
                {
                    // log failure
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                try
                {
                    EmailService emailService = new EmailService();
                    await emailService.SendPaymentLinkAsync(
                        request.Email,
                        request.CustomerName,
                        request.Amount,
                        stripe.Url);

                    emailSent = true;
                }
                catch
                {
                    // log email failure
                }
            }

            if (request.InvoiceId > 0)
            {
                _invoiceRepository.SavePaymentLink(
                    request.InvoiceId,
                    stripe.Id,
                    stripe.Url,
                    twilioSid,
                    whatsappSent,
                    emailSent);
            }
        }
    }
}
