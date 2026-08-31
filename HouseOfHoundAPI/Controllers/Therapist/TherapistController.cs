
using HouseOfHoundAPI.Data;
using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Therapist
{
   // [Authorize]
    [RoutePrefix("api/therapists")]
    public class TherapistsController : ApiController
    {
        private readonly ITherapistRepository _repo = new TherapistRepository(HohManager.GetConnectionString());

        public TherapistsController()
        {
            
        }
        public TherapistsController(ITherapistRepository repo)
        {
            _repo = repo;
        }

        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            return Ok(_repo.Get(id));
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost, Route("")]
        public IHttpActionResult Create(CreateTherapistDto dto)
        {
            var id = _repo.Create(dto);
            return Ok(new { TherapistId = id });
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, UpdateTherapistDto dto)
        {
            _repo.Update(id, dto);
            return Ok();
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            _repo.Delete(id);
            return Ok();
        }
    }
}
