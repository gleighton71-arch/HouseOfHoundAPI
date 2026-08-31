using HouseOfHoundAPI.Models.Payment;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Payments
{
    //[Authorize]
    [RoutePrefix("api/payments")]
    public class PaymentsController : ApiController
    {
        private readonly PaymentRequestService _service;

        public PaymentsController()
        {
            var cs = ConfigurationManager.ConnectionStrings["HoH"].ConnectionString;

            _service = new PaymentRequestService(
                new StripeService(),
                new TwilioWhatsAppService(),
                new InvoiceRepository());
        }

        [HttpPost]
        [Route("send-payment-link")]
        public async Task<IHttpActionResult> SendPaymentLink(SendInvoicePaymentLinkRequest request)
        {
            if (request == null)
                return BadRequest("Request body required.");

            await _service.SendInvoicePaymentLinkAsync(request);

            return Ok(new { Success = true });
        }
    }
}
