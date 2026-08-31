using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Models.Owner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfHoundAPI.Models.Dashboard
{ 
    public class CustomerDogProfile
    {
        public Owner.Owner owner { get; set; }
        public List<Dog.DogFullDto> dogs { get; set; } = new List<Dog.DogFullDto>();

        public List<Invoice.InvoiceSummaryDto> invoices { get; set; } = new List<Invoice.InvoiceSummaryDto>();
    }

    public class DogProfile
    {
        public Dog.DogFullDto dog { get; set; }
    }





}