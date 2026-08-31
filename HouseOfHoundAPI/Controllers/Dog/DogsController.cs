using HouseOfHoundAPI.Models.Dog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Dog
{
 //   [Authorize]
    [RoutePrefix("api/dogs")]
    public class DogsController : ApiController
    {
        private readonly DogRepository _repo = new DogRepository();
        [HttpGet, Route("")]
        public IHttpActionResult GetAll()
        {
            var result = _repo.GetDogs();

            return Ok(result); // TODO: return list
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            return Ok(); // TODO
        }

        [HttpPost, Route("")]
        public IHttpActionResult Create(CreateDogDto dto)
        {
            int result = _repo.CreateDog(dto);
            return Ok(result); // TODO
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Update(int id, CreateDogDto dto)
        {
            int result = _repo.UpdateDog(id, dto);
            return Ok(); // TODO
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            return Ok(); // TODO
        }
        [HttpGet, Route("{id:int}/bookings")]
        public IHttpActionResult GetDogBookings(int id)
        {
            var result = _repo.GetDogBookings(id);

            return Ok(result);
        }
    }
}
