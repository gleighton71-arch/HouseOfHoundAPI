using HouseOfHoundAPI.Models.SpecialistProviders;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.SpecialistProviders
{
    [RoutePrefix("api/specialistproviders")]
    public class SpecialistProvidersController : ApiController
    {
        private readonly SpecialistProviderRepository _repo = new SpecialistProviderRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var provider = _repo.Get(id);
            return provider == null ? (IHttpActionResult)NotFound() : Ok(provider);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(SpecialistProvider provider)
        {
            var validation = ValidateProvider(provider);
            if (validation != null) return validation;

            try
            {
                var id = _repo.Create(provider);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, SpecialistProvider provider)
        {
            var validation = ValidateProvider(provider);
            if (validation != null) return validation;

            try
            {
                return _repo.Update(id, provider) ? Ok() : (IHttpActionResult)NotFound();
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

        private IHttpActionResult ValidateProvider(SpecialistProvider provider)
        {
            if (provider == null) return BadRequest("Specialist provider is required.");
            if (string.IsNullOrWhiteSpace(provider.SpecialistId)) return BadRequest("Specialist ID is required.");
            if (string.IsNullOrWhiteSpace(provider.Name)) return BadRequest("Specialist name is required.");
            return null;
        }
    }
}
