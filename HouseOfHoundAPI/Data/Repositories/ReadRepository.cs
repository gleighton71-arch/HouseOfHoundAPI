using HouseOfHoundAPI.Controllers.Dog;
using HouseOfHoundAPI.Models;
using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Models.Equipment;
using HouseOfHoundAPI.Models.Invoice;
using HouseOfHoundAPI.Models.Owner;
using HouseOfHoundAPI.Models.Session;
using HouseOfHoundAPI.Models.Therapist;
using HouseOfHoundAPI.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Twilio.Rest.Trusthub.V1;

public class ReadRepository
{
    public DashboardSummaryDto GetDashboardSummary()
    {
        var result = new DashboardSummaryDto();

        using (var conn = Db.OpenConnection())
        {
            using (var cmd = new SqlCommand(@"
SELECT
    SUM(CASE WHEN Status = 'Booked' THEN 1 ELSE 0 END) AS Scheduled,
    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
    SUM(CASE WHEN Status IN ('Cancelled', 'FailedToAttend', 'Failed to attend', 'NoShow', 'No Show') THEN 1 ELSE 0 END) AS FailedToAttend
FROM dbo.Bookings
WHERE CAST(StartTimeUtc AS date) = CAST(SYSUTCDATETIME() AS date);", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    result.TodaysAppointments = new TodaysAppointmentSummaryDto
                    {
                        Scheduled = rdr["Scheduled"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Scheduled"]),
                        Completed = rdr["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Completed"]),
                        FailedToAttend = rdr["FailedToAttend"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["FailedToAttend"])
                    };
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT
    o.OwnerId,
    o.FullName AS OwnerName,
    i.InvoiceId,
    i.CreatedUtc AS InvoiceDate,
    i.TotalAmount,
    d.DogId,
    d.Name AS DogName
FROM dbo.Invoices i
JOIN dbo.Owners o ON o.OwnerId = i.OwnerId
LEFT JOIN dbo.Bookings b ON b.InvoiceId = i.InvoiceId
LEFT JOIN dbo.Dogs d ON d.DogId = b.DogId
WHERE i.Status NOT IN ('Paid', 'Voided')
ORDER BY o.FullName, i.CreatedUtc ASC, i.InvoiceId ASC;", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                var debtors = new Dictionary<int, DashboardDebtorDto>();

                while (rdr.Read())
                {
                    var ownerId = (int)rdr["OwnerId"];
                    DashboardDebtorDto debtor;
                    if (!debtors.TryGetValue(ownerId, out debtor))
                    {
                        debtor = new DashboardDebtorDto
                        {
                            OwnerId = ownerId,
                            OwnerName = rdr["OwnerName"]?.ToString()
                        };
                        debtors.Add(ownerId, debtor);
                    }

                    var invoiceDate = (DateTime)rdr["InvoiceDate"];
                    var amount = (decimal)rdr["TotalAmount"];
                    debtor.TotalOutstanding += amount;

                    var ageDays = Math.Max(0, (int)Math.Floor((DateTime.UtcNow - invoiceDate).TotalDays));
                    if (ageDays > debtor.OldestDebtAgeDays)
                    {
                        debtor.OldestDebtAgeDays = ageDays;
                    }

                    debtor.Invoices.Add(new DashboardDebtorInvoiceDto
                    {
                        DogId = rdr["DogId"] == DBNull.Value ? (int?)null : Convert.ToInt32(rdr["DogId"]),
                        DogName = rdr["DogName"]?.ToString(),
                        InvoiceId = (int)rdr["InvoiceId"],
                        InvoiceDate = invoiceDate,
                        Amount = amount
                    });
                }

                result.OutstandingDebtors = debtors.Values
                    .OrderByDescending(d => d.TotalOutstanding)
                    .ThenByDescending(d => d.OldestDebtAgeDays)
                    .ToList();
            }

            using (var cmd = new SqlCommand(@"
SELECT
    e.EquipmentId,
    s.EquipmentServiceScheduleId,
    e.Name AS EquipmentName,
    e.Category,
    s.ServiceName,
    s.ServiceDueDate
FROM dbo.EquipmentServiceSchedules s
JOIN dbo.Equipment e ON e.EquipmentId = s.EquipmentId
WHERE e.Active = 1
  AND s.Status = 'Service Due'
  AND s.BookedServiceDate IS NULL
  AND s.ServiceDueDate >= CAST(SYSUTCDATETIME() AS date)
  AND s.ServiceDueDate < DATEADD(month, 6, CAST(SYSUTCDATETIME() AS date))
ORDER BY s.ServiceDueDate ASC, e.Name ASC;", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var dueDate = (DateTime)rdr["ServiceDueDate"];
                    result.EquipmentServicesDue.Add(new DashboardEquipmentServiceDueDto
                    {
                        EquipmentId = (int)rdr["EquipmentId"],
                        EquipmentServiceScheduleId = (int)rdr["EquipmentServiceScheduleId"],
                        EquipmentName = rdr["EquipmentName"]?.ToString(),
                        ServiceName = rdr["ServiceName"]?.ToString(),
                        Category = rdr["Category"]?.ToString(),
                        ServiceDueDate = dueDate,
                        DaysUntilDue = Math.Max(0, (int)Math.Floor((dueDate.Date - DateTime.UtcNow.Date).TotalDays))
                    });
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT
    d.DogId,
    d.Name AS DogName,
    d.Breed,
    o.OwnerId,
    o.FullName AS OwnerName,
    MAX(pastBookings.StartTimeUtc) AS LastAppointmentDate
FROM dbo.Dogs d
JOIN dbo.Owners o ON o.OwnerId = d.OwnerId
LEFT JOIN dbo.Bookings futureBookings
    ON futureBookings.DogId = d.DogId
   AND futureBookings.StartTimeUtc >= SYSUTCDATETIME()
   AND futureBookings.Status <> 'Cancelled'
LEFT JOIN dbo.Bookings pastBookings
    ON pastBookings.DogId = d.DogId
   AND pastBookings.StartTimeUtc < SYSUTCDATETIME()
WHERE d.IsArchived = 0
  AND futureBookings.BookingId IS NULL
GROUP BY d.DogId, d.Name, d.Breed, o.OwnerId, o.FullName
ORDER BY LastAppointmentDate ASC, d.Name ASC;", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    result.DogsWithoutFutureAppointments.Add(new DashboardDogNoFutureAppointmentDto
                    {
                        DogId = (int)rdr["DogId"],
                        DogName = rdr["DogName"]?.ToString(),
                        Breed = rdr["Breed"]?.ToString(),
                        OwnerId = (int)rdr["OwnerId"],
                        OwnerName = rdr["OwnerName"]?.ToString(),
                        LastAppointmentDate = rdr["LastAppointmentDate"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["LastAppointmentDate"]
                    });
                }
            }

            using (var cmd = new SqlCommand(@"
SELECT
    Id AS StockItemId,
    Code,
    Description,
    QuantityInStock,
    MinimumStockHolding,
    MinimumStockHolding - QuantityInStock AS ReorderQuantity
FROM dbo.StockItem
WHERE IsActive = 1
  AND MinimumStockHolding > 0
  AND QuantityInStock <= MinimumStockHolding
ORDER BY (MinimumStockHolding - QuantityInStock) DESC, Code ASC;", conn))
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    result.StockNeedingReplenishment.Add(new DashboardStockReplenishmentDto
                    {
                        StockItemId = (int)rdr["StockItemId"],
                        Code = rdr["Code"]?.ToString(),
                        Description = rdr["Description"]?.ToString(),
                        QuantityInStock = (int)rdr["QuantityInStock"],
                        MinimumStockHolding = (int)rdr["MinimumStockHolding"],
                        ReorderQuantity = (int)rdr["ReorderQuantity"]
                    });
                }
            }
        }

        return result;
    }


    public List<TherapistDiaryEntryDto> GetTherapistDiary(int therapistId, DateTime? date)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
        SELECT 
            b.BookingId,
            d.Name AS DogName,
            o.FullName AS OwnerName,
            b.StartTimeUtc,
            b.EndTimeUtc,
            b.Status
        FROM dbo.Bookings b
        JOIN dbo.Dogs d ON d.DogId = b.DogId
        JOIN dbo.Owners o ON o.OwnerId = d.OwnerId
        WHERE b.TherapistId = @TherapistId
          AND ( @Date = '1901-01-01' or CAST(b.StartTimeUtc AS DATE) = @Date)
        ORDER BY b.StartTimeUtc;", conn))
        {
            cmd.Parameters.AddWithValue("@TherapistId", therapistId);
           
            if ( date == null || !date.HasValue )
            {
                date = DateTime.Parse("1901-01-01");
            }
            cmd.Parameters.AddWithValue("@Date", date);


            using (var rdr = cmd.ExecuteReader())
            {
                var list = new List<TherapistDiaryEntryDto>();
                while (rdr.Read())
                {
                    list.Add(new TherapistDiaryEntryDto
                    {
                        BookingId = (int)rdr["BookingId"],
                        DogName = rdr["DogName"] as string,
                        OwnerName = rdr["OwnerName"] as string,
                        StartTimeUtc = (DateTime)rdr["StartTimeUtc"],
                        EndTimeUtc = (DateTime)rdr["EndTimeUtc"],
                        Status = rdr["Status"] as string
                    });
                }
                return list;
            }
        }
    }



    public DogHistoryDto GetDogHistory(int dogId)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
        SELECT 
            d.DogId, 
            d.Name, 
            d.Breed,
            s.SessionId, 
            s.SessionDateUtc, 
            s.ClinicalNotes,
            t.Name AS TherapistName
        FROM dbo.Dogs d
        LEFT JOIN dbo.Bookings b ON b.DogId = d.DogId
        LEFT JOIN dbo.Sessions s ON s.BookingId = b.BookingId
        LEFT JOIN dbo.Therapists t ON t.TherapistId = b.TherapistId
        WHERE d.DogId = @DogId
        ORDER BY s.SessionDateUtc DESC;", conn))
        {
            cmd.Parameters.AddWithValue("@DogId", dogId);

            using (var rdr = cmd.ExecuteReader())
            {
                DogHistoryDto result = null;

                while (rdr.Read())
                {
                    if (result == null)
                    {
                        result = new DogHistoryDto
                        {
                            DogId = (int)rdr["DogId"],
                            Name = rdr["Name"] as string,
                            Breed = rdr["Breed"] as string
                        };
                    }

                    if (rdr["SessionId"] != DBNull.Value)
                    {
                        result.Sessions.Add(new DogSessionHistoryDto
                        {
                            SessionId = (int)rdr["SessionId"],
                            SessionDateUtc = rdr["SessionDateUtc"] as DateTime?,
                            ClinicalNotes = rdr["ClinicalNotes"] as string,
                            TherapistName = rdr["TherapistName"] as string
                        });
                    }
                }

                return result;
            }
        }
    }
    public List<dynamic> GetTherapistDiaryRaw(int therapistId, DateTime date)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
        SELECT 
            b.BookingId,
            d.Name AS DogName,
            o.FullName AS OwnerName,
            b.StartTimeUtc,
            b.EndTimeUtc,
            b.Status
        FROM dbo.Bookings b
        JOIN dbo.Dogs d ON d.DogId = b.DogId
        JOIN dbo.Owners o ON o.OwnerId = d.OwnerId
        WHERE b.TherapistId = @TherapistId
          AND CAST(b.StartTimeUtc AS DATE) = @Date
        ORDER BY b.StartTimeUtc;", conn))
        {
            cmd.Parameters.AddWithValue("@TherapistId", therapistId);
            cmd.Parameters.AddWithValue("@Date", date.Date);

            using (var rdr = cmd.ExecuteReader())
            {
                var list = new List<dynamic>();
                while (rdr.Read())
                {
                    list.Add(new
                    {
                        BookingId = rdr["BookingId"],
                        DogName = rdr["DogName"],
                        OwnerName = rdr["OwnerName"],
                        StartTimeUtc = rdr["StartTimeUtc"],
                        EndTimeUtc = rdr["EndTimeUtc"],
                        Status = rdr["Status"]
                    });
                }
                return list;
            }
        }
    }

    //public DogFullDto GetDogFullProfile(int dogId)
    //{
    //    using (var conn = Db.OpenConnection())
    //    using (var cmd = new SqlCommand(@"
    //    SELECT
    //        d.DogId,
    //        d.Name AS DogName,
    //        d.Breed,

    //        o.OwnerId,
    //        o.FullName,
    //        o.Email,

    //        b.BookingId,
    //        b.StartTimeUtc,
    //        b.Status,

    //        s.SessionId,
    //        s.SessionDateUtc,
    //        s.ClinicalNotes

    //    FROM Dogs d
    //    JOIN Owners o ON o.OwnerId = d.OwnerId
    //    LEFT JOIN Bookings b ON b.DogId = d.DogId
    //    LEFT JOIN Sessions s ON s.BookingId = b.BookingId
    //    WHERE d.DogId = @DogId
    //    ORDER BY b.StartTimeUtc DESC;", conn))
    //    {
    //        cmd.Parameters.AddWithValue("@DogId", dogId);

    //        using (var rdr = cmd.ExecuteReader())
    //        {
    //            DogFullDto dog = null;
    //            var bookingLookup = new Dictionary<int, HouseOfHoundAPI.Models.Booking.BookingSummaryDto>();

    //            while (rdr.Read())
    //            {
    //                if (dog == null)
    //                {
    //                    dog = new DogFullDto
    //                    {
    //                        DogId = (int)rdr["DogId"],
    //                        Name = rdr["DogName"].ToString(),
    //                        Breed = rdr["Breed"].ToString(),
    //                        Owner = new OwnerSummaryDto
    //                        {
    //                            OwnerId = (int)rdr["OwnerId"],
    //                            FullName = rdr["FullName"].ToString(),
    //                            Email = rdr["Email"].ToString()
    //                        }
    //                    };
    //                }

    //                if (rdr["BookingId"] != DBNull.Value)
    //                {
    //                    int bookingId = (int)rdr["BookingId"];

    //                    if (!bookingLookup.ContainsKey(bookingId))
    //                    {
    //                        var booking = new HouseOfHoundAPI.Models.Dog.BookingSummaryDto
    //                        {
    //                            BookingId = bookingId,
    //                            StartTimeUtc = (DateTime)rdr["StartTimeUtc"],
    //                            Status = rdr["Status"]?.ToString()
    //                        };

    //                        bookingLookup.Add(bookingId, booking);
    //                        dog.Bookings.Add(booking);
    //                    }

    //                    if (rdr["SessionId"] != DBNull.Value)
    //                    {
    //                        bookingLookup[bookingId].Sessions.Add(new SessionSummaryDto
    //                        {
    //                            SessionId = (int)rdr["SessionId"],
    //                            SessionDateUtc = rdr["SessionDateUtc"] as DateTime?,
    //                            ClinicalNotes = rdr["ClinicalNotes"]?.ToString()
    //                        });
    //                    }
    //                }
    //            }

    //            return dog;
    //        }
    //    }
    //}


    public OwnerDetailDto GetOwnerProfileRaw(int ownerId)
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
            SELECT o.OwnerId, o.FullName, o.Email,
                   d.DogId, d.Name AS DogName, d.Breed,d.DateOfBirth,
                   b.BookingId, b.StartTimeUtc, b.Status,
                   t.Name AS TherapistName,
                   i.InvoiceId, i.TotalAmount, i.Status AS InvoiceStatus,
                   o.Phone, o.Address 
            FROM dbo.Owners o
            LEFT JOIN dbo.Dogs d ON d.OwnerId = o.OwnerId
            LEFT JOIN dbo.Bookings b ON b.DogId = d.DogId AND b.StartTimeUtc >= SYSUTCDATETIME()
            LEFT JOIN dbo.Therapists t ON t.TherapistId = b.TherapistId
            LEFT JOIN dbo.Invoices i ON i.OwnerId = o.OwnerId
            WHERE o.OwnerId = @OwnerId;", conn))
        {
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);

            using (var rdr = cmd.ExecuteReader())
            {
                OwnerDetailDto owner = null;
                var dogLookup = new Dictionary<int, DogSummaryDto>();

                while (rdr.Read())
                {
                    if (owner == null)
                    {
                        owner = new OwnerDetailDto
                        {
                            OwnerId = (int)rdr["OwnerId"],
                            FullName = rdr["FullName"].ToString(),
                            Email = rdr["Email"]?.ToString(),
                            Phone = rdr["Phone"]?.ToString(),
                            Address = rdr["Address"]?.ToString(),
                            Dogs = new List<DogSummaryDto>()
                        };
                    }

                    if (rdr["DogId"] != DBNull.Value)
                    {
                        int dogId = (int)rdr["DogId"];

                        if (!dogLookup.ContainsKey(dogId))
                        {
                            var dog = new DogSummaryDto
                            {
                                DogId = dogId,
                                Name = rdr["DogName"]?.ToString(),
                                Breed = rdr["Breed"]?.ToString(),
                                DOB = Convert.ToDateTime(rdr["DateOfBirth"])
                            };

                            dogLookup.Add(dogId, dog);
                            owner.Dogs.Add(dog);
                        }
                    }
                }

                return owner;
            }
        }
    }

    public CustomerDogProfile GetCustomerDogProfile(int ownerId)
    {
        CustomerDogProfile customerDogProfile = new CustomerDogProfile();


        OwnerService ownerService = new OwnerService();
        Owner owner = ownerService.GetOwner(ownerId);


        //Owner owner = new Owner();
        //owner.OwnerId = ownerId;
        //owner.FullName = "John Doe";
        //owner.Email = "john.doe@email.com";
        //owner.Phone = "123-456-7890";
        //owner.Address = "123 Main St, Anytown, USA";
        
        customerDogProfile.owner = owner;

        DogRepository dogRepository = new DogRepository();

        List<DogDetailDto> dogs = dogRepository.GetDogsByOwnerId(ownerId);

        dogs.ForEach(dog => customerDogProfile.dogs.Add(new DogFullDto
        {
            DogId = dog.DogId,
            Name = dog.Name,
            Breed = dog.Breed,
            DateOfBirth = dog.DateOfBirth,
            WeightKg = dog.WeightKg,
            ImageURL = dog.ImageURL,
            MicroChip = dog.MicroChip,
            IsVetReferral = dog.IsVetReferral,
            IsArchived = dog.IsArchived,
            Owner = new OwnerSummaryDto()
            {
                Email = owner.Email,
                FullName = owner.FullName,
                OwnerId = ownerId,
                Phone = owner.Phone
            },
            Notes = dogRepository.GetDogNotes(dog.DogId)
        }));

        BookingService bookingService = new BookingService();
        List<Booking> bookings = bookingService.GetBookingsForOwner(ownerId);

        List<HouseOfHoundAPI.Models.Dog.BookingSummaryDto> bookingSummaries = bookings.Select(b => new HouseOfHoundAPI.Models.Dog.BookingSummaryDto
        {
            BookingId = b.BookingId,
            StartTimeUtc = b.StartTimeUtc,
            EndTimeUtc = b.EndTimeUtc,
            Status = b.Status,
            Notes = b.Notes
        }).ToList();

        var sessionService = new SessionService();
        bookingSummaries.ForEach(bs =>
        {
            List<SessionDetailDto> sessions = sessionService.GetSessionsForBooking(bs.BookingId);

            bs.Sessions = sessions.Select(s => new SessionSummaryDto
            {
                SessionId = s.SessionId,
                SessionDateUtc = s.SessionDateUtc,
                ClinicalNotes = s.ClinicalNotes,
            }).ToList();
        });

        InvoiceRepository invoiceRepository = new InvoiceRepository();
        customerDogProfile.invoices = invoiceRepository.GetInvoicesForOwner(ownerId);

        return customerDogProfile;
    }


    public List<Equipment> GetEquipment()
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
        SELECT 
            EquipmentId,
            Name,
            Category,
            HasValue,
            Value,
            SerialNumber,
            Status,
            Active,
            CreatedDate
        FROM Equipment", conn))
        {
            var list = new List<Equipment>();

            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    list.Add(new Equipment
                    {
                        EquipmentId = rdr.GetInt32(rdr.GetOrdinal("EquipmentId")),

                        Name = rdr["Name"] as string,

                        Category = rdr["Category"] as string,

                        HasValue = rdr.GetBoolean(rdr.GetOrdinal("HasValue")),

                        Value = rdr["Value"] != DBNull.Value
                            ? (decimal?)rdr["Value"]
                            : null,

                        SerialNumber = rdr["SerialNumber"] as string,

                        Status = rdr["Status"] as string,

                        Active = rdr.GetBoolean(rdr.GetOrdinal("Active")),

                        CreatedDate = rdr.GetDateTime(rdr.GetOrdinal("CreatedDate"))
                    });
                }
            }

            return list;
        }
    }

    public List<DogOwnerListItemDto> GetDogOwnersList()
    {
        using (var conn = Db.OpenConnection())
        using (var cmd = new SqlCommand(@"
select o.FullName,
o.OwnerId,
o.Phone,
o.Email,
d.Name,
d.Breed,
d.DogId,
case when b.NextBookingDate is not null then 
convert(char(17),b.NextBookingDate,13) 
else 
'Never' 
end as NextBooking ,
isnull(owed.Outstanding,0) as Outstanding,
'' as PlanName
from Owners o
join Dogs d on o.OwnerId = d.OwnerId
left join 
(
select distinct DogId,min(StartTimeUTC) as NextBookingDate from Bookings
where Status='Booked' 
group by DogId
) as b
on b.DogId = d.DogId
left join 
(
select distinct(OwnerId) as OwnerId,sum(TotalAmount) as Outstanding from Invoices where Status <> 'Paid'
group by OwnerId
) as owed
on owed.OwnerId = o.OwnerId
    ", conn))
        {

            List<DogOwnerListItemDto> dogOwnerListItemDto = new List<DogOwnerListItemDto>();
            var list = new List<DogOwnerListItemRaw>();

            using (var rdr = cmd.ExecuteReader())
            {
               

                while (rdr.Read())
                {


                    list.Add(new DogOwnerListItemRaw
                    {
                        OwnerId = (int)rdr["OwnerId"],
                        FullName = rdr["FullName"]?.ToString(),

                        DogId = (int)rdr["DogId"],
                        Name = rdr["Name"]?.ToString(),
                        Breed = rdr["Breed"]?.ToString(),

                        NextBooking = rdr["NextBooking"]?.ToString(),

                        Outstanding = (decimal)rdr["Outstanding"],
                        Email = rdr["Email"]?.ToString(),
                        Phone = rdr["Phone"]?.ToString(),
                        PlanName = rdr["PlanName"]?.ToString()
                    });
                }

            }

            dogOwnerListItemDto = list.GroupBy(x => x.OwnerId).Select(g => new DogOwnerListItemDto
            {
                OwnerId = g.Key,
                customerName = g.First().FullName,
                email = g.First().Email,
                phone = g.First().Phone,
                nextAppointment = new Appointment{ date =  g.First().NextBooking, service = "" },
                treatmentPlan = g.First().PlanName,
                outstanding = g.First().Outstanding,
                dogs = g.Select(d => new OwnedDogs
                {
                    Id = d.DogId,
                    name = d.Name,
                    breed = d.Breed
                }).ToList()
            }).ToList();
            return dogOwnerListItemDto;
        }

        
    
    }

     
}
