using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models
{ 
    public class Note
    {
        public int Id { get; set; }
        public int DogId { get; set; }
        public DateTime CreatedDateUTC { get; set; } = DateTime.Now;
        public string Content { get; set; }
        public bool RequiresAction { get; set; } 


    }
}