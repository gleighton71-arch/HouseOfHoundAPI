using HouseOfHoundAPI.Models.Equipment;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers
{
    //[Authorize]
    [RoutePrefix("api/equipment")]
    public class EquipmentController : ApiController
    {
        private readonly EquipmentService _service;

        public EquipmentController()
        {
            _service = new EquipmentService();
        }

        // 🔹 GET api/equipment?activeOnly=true
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = false)
        {
            var data = _service.GetEquipment(activeOnly);
            return Ok(data);
        }

        // 🔹 GET api/equipment/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var item = _service.GetEquipmentById(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // 🔹 POST api/equipment
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] Equipment model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = _service.InsertEquipment(model);

            return Ok(new { EquipmentId = newId });
        }

        // 🔹 PUT api/equipment/5
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] Equipment model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

            if (id != model.EquipmentId)
                return BadRequest("ID mismatch");

            var existing = _service.GetEquipmentById(id);
            if (existing == null)
                return NotFound();

            _service.UpdateEquipment(model);

            return Ok();
        }

        // 🔹 DELETE api/equipment/5 (soft delete)
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var existing = _service.GetEquipmentById(id);
            if (existing == null)
                return NotFound();

            _service.DeactivateEquipment(id);

            return Ok();
        }

        [HttpPost]
        [Route("{id:int}/service-schedules")]
        public IHttpActionResult AddServiceSchedule(int id, [FromBody] CreateEquipmentServiceDto model)
        {
            var validation = ValidateServiceSchedule(id, model);
            if (validation != null)
                return validation;

            var existing = _service.GetEquipmentById(id);
            if (existing == null)
                return NotFound();

            var scheduleId = _service.InsertServiceSchedule(model);
            return Ok(new { EquipmentServiceScheduleId = scheduleId });
        }

        [HttpPut]
        [Route("{id:int}/service-schedules/{scheduleId:int}")]
        public IHttpActionResult UpdateServiceSchedule(int id, int scheduleId, [FromBody] CreateEquipmentServiceDto model)
        {
            var validation = ValidateServiceSchedule(id, model);
            if (validation != null)
                return validation;

            var updated = _service.UpdateServiceSchedule(scheduleId, model);
            if (!updated)
                return NotFound();

            return Ok();
        }

        private IHttpActionResult ValidateServiceSchedule(int equipmentId, CreateEquipmentServiceDto model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

            if (equipmentId != model.EquipmentId)
                return BadRequest("ID mismatch");

            if (model.ServiceDueDate == default(DateTime))
                return BadRequest("Service due date is required");

            if (model.Status != "Service Due" && model.Status != "Service Booked" && model.Status != "Completed")
                return BadRequest("Invalid service status");

            if (model.Status == "Service Booked" && !model.BookedServiceDate.HasValue)
                return BadRequest("Booked service date is required when status is Service Booked");

            if (model.Status == "Completed" && !model.ServiceDate.HasValue)
                return BadRequest("Service date is required when status is Completed");

            return null;
        }
    }
}
