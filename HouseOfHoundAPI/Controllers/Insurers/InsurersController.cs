using HouseOfHoundAPI.Models.Insurers;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Insurers
{
    [RoutePrefix("api/insurers")]
    public class InsurersController : ApiController
    {
        private readonly InsurerRepository _repo = new InsurerRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var insurer = _repo.Get(id);
            return insurer == null ? (IHttpActionResult)NotFound() : Ok(insurer);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(Insurer insurer)
        {
            var validation = ValidateInsurer(insurer);
            if (validation != null) return validation;

            try
            {
                var id = _repo.Create(insurer);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, Insurer insurer)
        {
            var validation = ValidateInsurer(insurer);
            if (validation != null) return validation;

            try
            {
                return _repo.Update(id, insurer) ? Ok() : (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route("{insurerId:int}/policies")]
        public IHttpActionResult GetPolicies(int insurerId)
        {
            return Ok(_repo.GetPolicies(insurerId));
        }

        [HttpGet, Route("policies/{policyId:int}")]
        public IHttpActionResult GetPolicy(int policyId)
        {
            var policy = _repo.GetPolicy(policyId);
            return policy == null ? (IHttpActionResult)NotFound() : Ok(policy);
        }

        [HttpPost, Route("{insurerId:int}/policies")]
        public IHttpActionResult CreatePolicy(int insurerId, InsurerPolicy policy)
        {
            var validation = ValidatePolicy(policy);
            if (validation != null) return validation;

            try
            {
                var id = _repo.CreatePolicy(insurerId, policy);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route("policies/{policyId:int}")]
        public IHttpActionResult UpdatePolicy(int policyId, InsurerPolicy policy)
        {
            var validation = ValidatePolicy(policy);
            if (validation != null) return validation;

            try
            {
                return _repo.UpdatePolicy(policyId, policy) ? Ok() : (IHttpActionResult)NotFound();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private IHttpActionResult ValidateInsurer(Insurer insurer)
        {
            if (insurer == null) return BadRequest("Insurer is required.");
            if (string.IsNullOrWhiteSpace(insurer.InsurerId)) return BadRequest("Insurer ID is required.");
            if (string.IsNullOrWhiteSpace(insurer.Name)) return BadRequest("Insurer name is required.");
            return null;
        }

        private IHttpActionResult ValidatePolicy(InsurerPolicy policy)
        {
            if (policy == null) return BadRequest("Policy is required.");
            if (string.IsNullOrWhiteSpace(policy.PolicyId)) return BadRequest("Policy ID is required.");
            if (string.IsNullOrWhiteSpace(policy.Name)) return BadRequest("Policy name is required.");
            return null;
        }
    }
}
