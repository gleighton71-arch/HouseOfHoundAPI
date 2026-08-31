using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Dog
{
    [RoutePrefix("api/dogs/{dogId:int}/treatmentplans")]
    public class DogTreatmentPlanAssignmentsController : ApiController
    {
        private readonly DogTreatmentPlanAssignmentRepository _repo =
            new DogTreatmentPlanAssignmentRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult GetHistory(int dogId)
        {
            return Ok(_repo.GetHistory(dogId));
        }

        [HttpPost, Route("")]
        public IHttpActionResult Assign(int dogId, CreateDogTreatmentPlanAssignmentRequest request)
        {
            if (request == null) return BadRequest("Treatment plan assignment is required.");
            if (request.TreatmentPlanId <= 0) return BadRequest("Treatment plan is required.");

            try
            {
                var id = _repo.Assign(dogId, request);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("{assignmentId:int}/complete")]
        public IHttpActionResult Complete(int dogId, int assignmentId, CompleteDogTreatmentPlanAssignmentRequest request)
        {
            try
            {
                var completed = _repo.Complete(dogId, assignmentId, request?.CompletedDateUtc);
                return completed ? Ok() : (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
