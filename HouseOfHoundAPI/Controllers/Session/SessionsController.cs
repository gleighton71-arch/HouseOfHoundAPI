using HouseOfHoundAPI.Models.Session;
using HouseOfHoundAPI.Services;
using System;
using System.Linq;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Session
{
    [RoutePrefix("api/sessions")]
    public class SessionsController : ApiController
    {
        private readonly SessionService _sessionService = new SessionService();
        private readonly SessionRepository _sessionRepository = new SessionRepository();

        [HttpGet, Route("{bookingId:int}")]
        public IHttpActionResult GetByBooking(int bookingId)
        {
            return Ok(_sessionService.GetSessionsForBooking(bookingId));
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(CreateSessionDto dto)
        {
            var id = _sessionService.CreateSession(dto);
            if (id <= 0)
                return BadRequest("Session could not be created.");

            return Ok(new { SessionId = id });
        }

        [HttpGet, Route("worklist")]
        public IHttpActionResult GetWorklist(int therapistId, DateTime date)
        {
            if (therapistId <= 0)
                return BadRequest("A therapist is required.");

            return Ok(_sessionRepository.GetTherapistWorklist(therapistId, date));
        }

        [HttpPost, Route("bookings/{bookingId:int}/status")]
        public IHttpActionResult UpdateBookingStatus(int bookingId, UpdateSessionBookingStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest("Status is required.");

            var status = request.Status.Trim();
            var allowedStatuses = new[] { "Booked", "In Progress", "Completed" };
            if (!allowedStatuses.Any(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Status is not valid for a session.");

            var normalizedStatus = allowedStatuses.First(s => string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
            if (!_sessionRepository.UpdateBookingStatus(bookingId, normalizedStatus))
                return NotFound();

            return Ok(new { BookingId = bookingId, Status = normalizedStatus });
        }
    }
}
