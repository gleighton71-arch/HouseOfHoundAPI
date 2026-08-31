using HouseOfHoundAPI.Models.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Invoice
{
    //[Authorize]
    [RoutePrefix("api/invoices")]
    public class InvoicesController : ApiController
    {
        private readonly InvoiceRepository _repo = new InvoiceRepository();

        [HttpGet, Route("owner/{ownerId:int}")]
        public IHttpActionResult GetByOwner(int ownerId, [FromUri] int? dogId = null, [FromUri] DateTime? fromDate = null, [FromUri] DateTime? toDate = null, [FromUri] string dateType = "invoice", [FromUri] string status = null, [FromUri] string search = null)
        {
            return Ok(_repo.GetInvoicesForOwner(ownerId, dogId, fromDate, toDate, dateType, status, search));
        }

        [HttpPost, Route("{invoiceId:int}/mark-paid")]
        public IHttpActionResult MarkPaid(int invoiceId)
        {
            _repo.MarkPaid(invoiceId, "Manual");
            return Ok(new { Success = true });
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(CreateInvoiceDto dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            // Validate the invoice lines
            if (dto.Lines == null || !dto.Lines.Any())
            {
                return BadRequest("Invoice must have at least one line.");
            }

            return Ok();


        }
    }
}
