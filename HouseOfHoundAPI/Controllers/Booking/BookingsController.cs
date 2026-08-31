using HouseOfHound.Api;
using HouseOfHound.Api.Repositories;
using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Session;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;

namespace HouseOfHoundAPI.Controllers.Booking
{
   

    //[Authorize]
    [RoutePrefix("api/bookings")]
    public partial class BookingsController : ApiController
    {
        private BookingService bookingService = new BookingService();
        private SessionService sessionService = new SessionService();
        private OwnerService ownerService = new OwnerService();
        private EmailService emailService = new EmailService();
        private DogService dogService = new DogService();

        InvoiceRepository invoiceRepository = new InvoiceRepository();

        [HttpGet, Route("")]
        public IHttpActionResult GetAll(DateTime? day = null)
        {
            var bookings = bookingService.GetAllBookings(day);
            return Ok(bookings);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var booking = bookingService.GetBookingSummary(id);
            if (booking == null) return NotFound();
            return Ok(booking);

        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> CreateAsync(CreateBookingDto dto)
        {

            Models.Owner.Owner ownerdetail = ownerService.GetOwnerByDogId(dto.DogId);
            Models.Dog.Dog dogdetail = dogService.GetDogById(dto.DogId);

            
            SqlConnection sqlConnection = HohManager.GetOpenConnection();

            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

            var invoiceId = invoiceRepository.CreateInvoice(sqlConnection, sqlTransaction, ownerdetail.OwnerId, dto.Cost.Value, "Draft");
            invoiceRepository.AddInvoiceLine(sqlConnection, sqlTransaction, invoiceId, $"Booking for {dogdetail.Name} on {dto.StartTimeUtc.Value.ToString("dd/MM/yyyy HH:mm")}", dto.Cost.Value);

            try
            {
                sqlTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqlTransaction.Rollback();
                return InternalServerError(new Exception("Booking created, but failed to create invoice"));
            }
            dto.InvoiceId = invoiceId;

            var id = bookingService.CreateBooking(dto);
            if ( id > 0 )
            {
                CreateSessionDto sessionDto = new CreateSessionDto
                {
                    BookingId = id,
                    SessionDateUtc = dto.StartTimeUtc,
                };
                int sessionId = sessionService.CreateSession(sessionDto);
                if ( sessionId <= 0)
                {
                    return InternalServerError(new Exception("Booking created, but failed to create session"));
                }

              

                string emailBody = $"<h2>Booking Confirmation</h2>" +

                    $"Dear {ownerdetail.FullName},\n\n" +
                    $"Your booking for {dogdetail.Name} on {dto.StartTimeUtc.Value.ToString("dd/MM/yyyy HH:mm")} has been confirmed.\n\n" +
                    $"Thank you for choosing House of Hound!" +
                    $"<p><em>Please note any cancellation must be made prior to {dto.StartTimeUtc.Value.AddDays(-3).Date.ToString("dd/MM/yyyy")}</em>";
                try
                {
                    await emailService.SendEmail(ownerdetail.Email, "House of Hound - Booking Confirmation", emailBody);

                    NoteRepository noteRepository = new NoteRepository();

                    noteRepository.CreateNote(new Models.Note
                    {
                        DogId = dto.DogId,
                        Content = $"Booking Email sent to {ownerdetail.Email}.",
                        CreatedDateUTC = DateTime.UtcNow,
                    });
                }
                catch (Exception ex)
                {
                    // Log the exception (not implemented here)
                    return InternalServerError(new Exception("Booking created, but failed to send confirmation email"));
                }

             





            }
            return Ok(new { id , invoiceId });
        }

        [HttpPost, Route("{id:int}/completed")]
        public IHttpActionResult Completed(int id)
        {
            bookingService.MarkBookingAsCompleted(id);
            return Ok(id); 
        }

        [HttpPost, Route("{id:int}/cancelled")]
        public IHttpActionResult Cancelled(int id)
        {
            bookingService.MarkBookingAsCancelled(id);
            return Ok(id);
        }

        [HttpPost, Route("{id:int}/created")]
        public IHttpActionResult Created(int id)
        {
            bookingService.MarkBookingAsCreated(id);
            return Ok(id);
        }

        [HttpPost, Route("{id:int}/inprogress")]
        public IHttpActionResult InProgress(int id)
        {
            bookingService.MarkBookingAsInProgress(id);
            return Ok(id);
        }



        [HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> UpdateAsync(int id, CreateBookingDto dto)
        {
            var result = bookingService.UpdateBooking(id, dto);
            if (result == false)
            {
                return InternalServerError(new Exception("Failed to update booking"));
            }

            Models.Owner.Owner ownerdetail = ownerService.GetOwnerByDogId(dto.DogId);
            Models.Dog.Dog dogdetail = dogService.GetDogById(dto.DogId);

            string emailBody = $"<h2>Booking Update Confirmation</h2>" +

                $"Dear {ownerdetail.FullName},\n\n" +
                $"Your booking for {dogdetail.Name} on {dto.StartTimeUtc.Value.ToString("dd/MM/yyyy HH:mm")} has been confirmed.\n\n" +
                $"Thank you for choosing House of Hound!" +
                $"<p><em>Please note any cancellation must be made prior to {dto.StartTimeUtc.Value.AddDays(-3).Date.ToString("dd/MM/yyyy")}</em>";
            try
            {
                await emailService.SendEmail(ownerdetail.Email, "House of Hound - Booking Confirmation", emailBody);

                NoteRepository noteRepository = new NoteRepository();

                noteRepository.CreateNote(new Models.Note
                {
                    DogId = dto.DogId,
                    Content = $"Booking Email sent to {ownerdetail.Email}.",
                    CreatedDateUTC = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                // Log the exception (not implemented here)
                return InternalServerError(new Exception("Booking created, but failed to send confirmation email"));
            }
            return Ok(id);
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id) {

            var resultSession = sessionService.RemoveSession(id);
            if ( resultSession == false)
            {
                return InternalServerError(new Exception("Failed to remove session"));
            }
            var result = bookingService.RemoveBooking(id);
            if ( result  == false )
            {
                return InternalServerError(new Exception("Failed to remove booking"));
            }

            return Ok();
        }
        [HttpPost, Route("available-slots")]
        public async Task<IHttpActionResult> GetAvailableSlotAsync(AvailableSlotRequest request)
        {

            var _repo = new BookingRepository();
            var availableSlots = await _repo.GetAvailableAppointmentTimesAsync(request.TherapistId, request.DogId, request.DurationMinutes, request.Day, request.IntervalMinutes); 
            return Ok(availableSlots);
        }
    }
}
