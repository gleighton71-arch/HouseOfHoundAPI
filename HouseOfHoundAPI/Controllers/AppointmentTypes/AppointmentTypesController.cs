using HouseOfHoundAPI.Models.AppointmentTypes;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.AppointmentTypes
{
    [RoutePrefix("api/appointmenttypes")]
    public class AppointmentTypesController : ApiController
    {
        private readonly AppointmentTypeRepository _repo = new AppointmentTypeRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var type = _repo.Get(id);
            return type == null ? (IHttpActionResult)NotFound() : Ok(type);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(AppointmentType type)
        {
            var validation = ValidateAppointmentType(type);
            if (validation != null) return validation;

            try
            {
                var id = _repo.Create(type);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, AppointmentType type)
        {
            var validation = ValidateAppointmentType(type);
            if (validation != null) return validation;

            try
            {
                return _repo.Update(id, type) ? Ok() : (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                return _repo.Delete(id) ? Ok() : (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private IHttpActionResult ValidateAppointmentType(AppointmentType type)
        {
            if (type == null) return BadRequest("Appointment type is required.");
            if (string.IsNullOrWhiteSpace(type.Code)) return BadRequest("Code is required.");
            if (string.IsNullOrWhiteSpace(type.Description)) return BadRequest("Description is required.");
            if (type.Cost < 0) return BadRequest("Cost cannot be negative.");
            if (type.DurationMinutes <= 0) return BadRequest("Duration must be greater than zero.");
            return null;
        }
    }
}
