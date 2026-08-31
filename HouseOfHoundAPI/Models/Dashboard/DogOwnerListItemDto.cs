using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio.Rest.Messaging.V1;

namespace HouseOfHoundAPI.Models.Dashboard
{
    
    public class Appointment
    {
        public string date { get; set; }
        public string service { get; set; }
    }

    public class OwnedDogs
    {
        public int Id { get; set; }
        public string name;
        public string breed;    
    }

    public class DogOwnerListItemDto
    {
        public int OwnerId { get; set; }
        public string customerName { get; set; }
        public List<OwnedDogs> dogs { get; set; }
        public string email { get; set; }
        public string phone { get; set; }

        public Appointment nextAppointment { get; set; }
        public string treatmentPlan { get; set; }

        public decimal outstanding { get; set; }

        public DogOwnerListItemDto()
        {
            nextAppointment = new Appointment();
            dogs = new List<OwnedDogs>();
        }
    }
    public class DogOwnerListItemRaw
    {
        public string FullName { get; set; }
        public int OwnerId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public int DogId { get; set; }

        public string NextBooking { get; set; }
        public string PlanName { get; set; }      
        public decimal Outstanding { get; set; }

    }

    public class DashboardSummaryDto
    {
        public TodaysAppointmentSummaryDto TodaysAppointments { get; set; }
        public List<DashboardDebtorDto> OutstandingDebtors { get; set; }
        public List<DashboardEquipmentServiceDueDto> EquipmentServicesDue { get; set; }
        public List<DashboardDogNoFutureAppointmentDto> DogsWithoutFutureAppointments { get; set; }
        public List<DashboardStockReplenishmentDto> StockNeedingReplenishment { get; set; }

        public DashboardSummaryDto()
        {
            TodaysAppointments = new TodaysAppointmentSummaryDto();
            OutstandingDebtors = new List<DashboardDebtorDto>();
            EquipmentServicesDue = new List<DashboardEquipmentServiceDueDto>();
            DogsWithoutFutureAppointments = new List<DashboardDogNoFutureAppointmentDto>();
            StockNeedingReplenishment = new List<DashboardStockReplenishmentDto>();
        }
    }

    public class TodaysAppointmentSummaryDto
    {
        public int Scheduled { get; set; }
        public int Completed { get; set; }
        public int FailedToAttend { get; set; }
    }

    public class DashboardDebtorDto
    {
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int OldestDebtAgeDays { get; set; }
        public List<DashboardDebtorInvoiceDto> Invoices { get; set; }

        public DashboardDebtorDto()
        {
            Invoices = new List<DashboardDebtorInvoiceDto>();
        }
    }

    public class DashboardDebtorInvoiceDto
    {
        public int? DogId { get; set; }
        public string DogName { get; set; }
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class DashboardEquipmentServiceDueDto
    {
        public int EquipmentId { get; set; }
        public int EquipmentServiceScheduleId { get; set; }
        public string EquipmentName { get; set; }
        public string ServiceName { get; set; }
        public string Category { get; set; }
        public DateTime ServiceDueDate { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class DashboardDogNoFutureAppointmentDto
    {
        public int DogId { get; set; }
        public string DogName { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Breed { get; set; }
        public DateTime? LastAppointmentDate { get; set; }
    }

    public class DashboardStockReplenishmentDto
    {
        public int StockItemId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int QuantityInStock { get; set; }
        public int MinimumStockHolding { get; set; }
        public int ReorderQuantity { get; set; }
    }
}
