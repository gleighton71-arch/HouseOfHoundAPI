using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseOfHoundAPI.Controllers.Read
{
    //[Authorize]
    [RoutePrefix("api/read")]
    public class ReadController : ApiController
    {
        private readonly ReadRepository _repo = new ReadRepository();

        [HttpGet, Route("dashboard")]
        public IHttpActionResult GetDashboard()
        {
            return Ok(_repo.GetDashboardSummary());
        }

        [HttpGet, Route("dogs/{dogId:int}/history")]
        public IHttpActionResult GetDogHistory(int dogId)
        {
            var result = _repo.GetDogHistory(dogId);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet, Route("owners/{ownerId:int}/profile")]
        public IHttpActionResult GetOwnerProfile(int ownerId)
        {
            var raw = _repo.GetOwnerProfileRaw(ownerId);
            return Ok(raw); // map to DTO later
        }

        //[HttpGet, Route("dogs/{dogId:int}/FullProfile")]
        //public IHttpActionResult GetDogFullProfile(int dogId)
        //{
        //    var raw = _repo.GetDogFullProfile(dogId);
        //    return Ok(raw); // map to DTO later
        //}


        [HttpGet, Route("dogswithowners")]
        public IHttpActionResult GetDogOwnersList()
        {
            var raw = _repo.GetDogOwnersList();
            return Ok(raw); // map to DTO later
        }



        [HttpGet, Route("customerswithdogs/{ownerId}")]
        public IHttpActionResult CustomersWithDogs(int ownerId)
        {
            CustomerDogProfile raw = _repo.GetCustomerDogProfile(ownerId);
            return Ok(raw); 
        }



        [HttpGet, Route("therapists/{therapistId:int}/diary")]
        public IHttpActionResult GetTherapistDiary(int therapistId, [FromUri] DateTime? date)
        {
            var result = _repo.GetTherapistDiary(therapistId, date);
            return Ok(result.OrderBy(o=>o.StartTimeUtc));
        }

     



    }
}
