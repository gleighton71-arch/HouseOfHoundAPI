using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Controllers.Dog
{
    public class DogHistoryDto
    {
        public int DogId { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public List<DogSessionHistoryDto> Sessions { get; set; } = new List<DogSessionHistoryDto>();
    }
}