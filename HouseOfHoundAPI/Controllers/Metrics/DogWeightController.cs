using HouseOfHound.Api.Repositories;
using HouseOfHoundAPI.Models.Metrics;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace HouseOfHound.Api.Controllers
{
    [RoutePrefix("api/dogweight")]
    public class DogWeightController : ApiController
    {
        private readonly DogWeightRepository _dogWeightRepository;

        public DogWeightController()
        {
            _dogWeightRepository = new DogWeightRepository();
        }

        // GET api/dogweight/dog/5
        [HttpGet]
        [Route("dog/{dogId:int}")]
        public IHttpActionResult GetDogWeights(int dogId)
        {
            try
            {
                List<DogWeight> weights = _dogWeightRepository.GetDogWeights(dogId);

                return Ok(weights);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/dogweight/10
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetDogWeightById(int id)
        {
            try
            {
                DogWeight dogWeight = _dogWeightRepository.GetDogWeightById(id);

                if (dogWeight == null)
                    return NotFound();

                return Ok(dogWeight);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/dogweight
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateDogWeight([FromBody] DogWeight dogWeight)
        {
            try
            {
                if (dogWeight == null)
                    return BadRequest("Dog weight cannot be null.");

                if (dogWeight.DogId <= 0)
                    return BadRequest("DogId is required.");

                if (dogWeight.WeightKg <= 0)
                    return BadRequest("WeightKg must be greater than zero.");

                DogWeight createdDogWeight = _dogWeightRepository.CreateDogWeight(dogWeight);

                return Content(HttpStatusCode.Created, createdDogWeight);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/dogweight/10
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateDogWeight(int id, [FromBody] DogWeight dogWeight)
        {
            try
            {
                if (dogWeight == null)
                    return BadRequest("Dog weight cannot be null.");

                if (id != dogWeight.Id)
                    return BadRequest("The weight ID in the URL does not match the weight ID in the body.");

                if (dogWeight.WeightKg <= 0)
                    return BadRequest("WeightKg must be greater than zero.");

                bool updated = _dogWeightRepository.UpdateDogWeight(dogWeight);

                if (!updated)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/dogweight/10
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteDogWeight(int id)
        {
            try
            {
                bool deleted = _dogWeightRepository.DeleteDogWeight(id);

                if (!deleted)
                    return NotFound();

                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}