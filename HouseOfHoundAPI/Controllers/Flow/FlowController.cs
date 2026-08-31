using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Models.Invoice;
using HouseOfHoundAPI.Models.Owner;
using HouseOfHoundAPI.Models.Session;
using System.Web.Http;


[RoutePrefix("api/flow")]
public class FlowController : ApiController
{
    private readonly HoHFlowService _service = new HoHFlowService();

    [HttpPost, Route("create-owner-booking-invoice")]
    public IHttpActionResult CreateOwnerBookingInvoice(CreateFullFlowDto dto)
    {
        if (dto == null) return BadRequest("Body required.");
        if (dto.Owner == null) return BadRequest("Owner required.");
        if (dto.Dog == null) return BadRequest("Dog required.");
        if (dto.Booking == null) return BadRequest("Booking required.");
        if (dto.Invoice == null) return BadRequest("Invoice required.");

        // Basic validation examples
        if (string.IsNullOrWhiteSpace(dto.Owner.FullName)) return BadRequest("Owner.FullName required.");
        if (string.IsNullOrWhiteSpace(dto.Dog.Name)) return BadRequest("Dog.Name required.");
        if (dto.Booking.EndTimeUtc <= dto.Booking.StartTimeUtc) return BadRequest("Booking end must be after start.");
        if (dto.Invoice.Lines == null || dto.Invoice.Lines.Count == 0) return BadRequest("Invoice.Lines required.");

        var result = _service.CreateOwnerDogBookingSessionInvoice(
            dto.Owner,
            dto.Dog,
            dto.Booking,
            dto.Session ?? new CreateSessionDto { ClinicalNotes = null },
            dto.Invoice
        );

        return Ok(result);
    }
}

public class CreateFullFlowDto
{
    public CreateOwnerDto Owner { get; set; }
    public CreateDogDto Dog { get; set; }
    public CreateBookingDto Booking { get; set; }
    public CreateSessionDto Session { get; set; } // optional
    public CreateInvoiceDto Invoice { get; set; }
}