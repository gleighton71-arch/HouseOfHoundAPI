using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Models.Owner;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Owner
{
  
    //[Authorize]
    [RoutePrefix("api/owners")]
    public class OwnersController : ApiController
    {
        private OwnerRepository _repo = new OwnerRepository(HohManager.GetOpenConnection());

        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var result = _repo.GetOwners();
            return Ok(result);

        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var result = _repo.GetOwners();
            if ( !result.Any())
            {
                return NotFound();
            }
            var entry = result.Find(o => o.OwnerId == id);
            if ( entry == null )
            {
                return NotFound();

            }
            return Ok(entry);

        }

        [HttpPost, Route("createwithdog")]
        public IHttpActionResult Create(NewOwnerWithDogRequest dto)
        {
            int result = _repo.CreateOwnerWithDog(dto);
            return Ok(result);
        }


        [HttpPost, Route("")]
        public IHttpActionResult Create(CreateOwnerDto dto)
        {
            int result = _repo.CreateOwner(dto);
            return Ok(result);
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, CreateOwnerDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }
            bool result = _repo.UpdateOwner(dto);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id) => Ok();
    }
}
