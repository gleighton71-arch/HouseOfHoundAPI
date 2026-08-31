using HouseOfHoundAPI.Models.Vets;
using HouseOfHoundAPI.Services;
using System;
using System.Net;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Vets
{
    [RoutePrefix("api/vets")]
    public class VetsController : ApiController
    {
        private readonly VetRepository _repo = new VetRepository(HohManager.GetConnectionString());

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var vet = _repo.Get(id);
            return vet == null ? (IHttpActionResult)NotFound() : Ok(vet);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(Vet vet)
        {
            var validation = ValidateVet(vet);
            if (validation != null) return validation;

            try
            {
                var id = _repo.Create(vet);
                return Content(HttpStatusCode.Created, new { Id = id });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, Vet vet)
        {
            var validation = ValidateVet(vet);
            if (validation != null) return validation;

            try
            {
                return _repo.Update(id, vet) ? Ok() : (IHttpActionResult)NotFound();
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

        private IHttpActionResult ValidateVet(Vet vet)
        {
            if (vet == null) return BadRequest("Vet is required.");
            if (string.IsNullOrWhiteSpace(vet.VetId)) return BadRequest("Vet ID is required.");
            if (string.IsNullOrWhiteSpace(vet.Name)) return BadRequest("Vet name is required.");
            return null;
        }
    }
}
