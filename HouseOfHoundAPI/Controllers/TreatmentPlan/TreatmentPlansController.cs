using HouseOfHoundAPI.Models.Treatment;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.TreatmentPlan
{
    [RoutePrefix("api/treatmentplans")]
    public class TreatmentPlansController : ApiController
    {
        private readonly TreatmentPlanService _service;

        public TreatmentPlansController()
        {
            _service = new TreatmentPlanService();
        }

        // 🔹 GET api/treatmentplans
        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            var plans = _service.GetTreatmentPlans();
            return Ok(plans);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] TreatmentPlanDto model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = _service.CreateTreatmentPlan(model);

            return Ok(new { TreatmentPlanId = newId });
        }

        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] TreatmentPlanDto model)
        {
            if (model == null)
                return BadRequest("Invalid payload");

           

            _service.UpdateTreatmentPlan(id, model);

            return Ok();
        }
    }
}
