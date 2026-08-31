using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Dog
{
    [RoutePrefix("api/dogs/{dogId:int}/providers")]
    public class DogProviderAssignmentsController : ApiController
    {
        private readonly DogProviderAssignmentRepository _repo = new DogProviderAssignmentRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult GetHistory(int dogId)
        {
            return Ok(_repo.GetHistory(dogId));
        }

        [HttpGet, Route("current")]
        public IHttpActionResult GetCurrent(int dogId)
        {
            var current = _repo.GetCurrent(dogId);
            return current == null ? (IHttpActionResult)NotFound() : Ok(current);
        }

        [HttpPost, Route("")]
        public IHttpActionResult SetCurrent(int dogId, DogProviderAssignment assignment)
        {
            if (assignment == null) return BadRequest("Provider assignment is required.");
            if (assignment.InsurerPolicyRecordId.HasValue && !assignment.InsurerRecordId.HasValue)
            {
                return BadRequest("Select an insurer before selecting a policy.");
            }

            try
            {
                assignment.DogId = dogId;
                var id = _repo.SetCurrent(dogId, assignment);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
