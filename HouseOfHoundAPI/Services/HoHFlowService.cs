using HouseOfHoundAPI.Models.Booking;
using HouseOfHoundAPI.Models.Dashboard;
using HouseOfHoundAPI.Models.Dog;
using HouseOfHoundAPI.Models.Invoice;
using HouseOfHoundAPI.Models.Owner;
using HouseOfHoundAPI.Models.Session;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


public class HoHFlowService
{
    private readonly OwnerRepository _owners = new OwnerRepository();
    private readonly DogRepository _dogs = new DogRepository();
    private readonly BookingRepository _bookings = new BookingRepository();
    private readonly SessionRepository _sessions = new SessionRepository();
    private readonly InvoiceRepository _invoices = new InvoiceRepository();

    private bool TherapistExists(SqlConnection conn, SqlTransaction tx, int therapistId)
    {
        using (var cmd = new SqlCommand("SELECT 1 FROM dbo.Therapists WHERE TherapistId = @Id", conn, tx))
        {
            cmd.Parameters.AddWithValue("@Id", therapistId);
            return cmd.ExecuteScalar() != null;
        }
    }

 


    public FlowResult CreateOwnerDogBookingSessionInvoice(
        CreateOwnerDto owner,
        CreateDogDto dog,
        CreateBookingDto booking,
        CreateSessionDto session,
        CreateInvoiceDto invoice)
    {
        using (var conn = Db.OpenConnection())
        using (var tx = conn.BeginTransaction())
        {
            try
            {

                if (!TherapistExists(conn, tx, booking.TherapistId))
                    throw new Exception("Therapist does not exist.");
                // 1) Owner
                int ownerId = 0;

                if (owner.Id > 0)
                {
                    if (!_owners.OwnerExists(conn, tx, owner.Id))
                    {
                        owner.Id = _owners.CreateOwner(conn, tx, owner);
                    }
                }
                if ( owner.Id == 0 )
                {
                    owner.Id = _owners.CreateOwner(conn, tx, owner);
                }

                // 2) Dog (force correct OwnerId)
                dog.OwnerId = owner.Id;

                if ( dog.OwnerId > 0 && dog.Id > 0 )
                {
                    int? dogowner = _dogs.GetOwnerIdForDog(conn, tx, dog.Id);
                    if (dogowner != null)
                    {
                        if ( dogowner != dog.OwnerId)
                        {
                            var dogId = _dogs.CreateDog(conn, tx, dog);
                            dog.Id = dogId;
                        }
                    }
                }

                // 3) Booking (force correct DogId)
                booking.DogId = dog.Id;
                var bookingId = _bookings.CreateBooking(conn, tx, booking);

                // 4) Session (force correct BookingId)
                session.BookingId = bookingId;
                var sessionId = _sessions.CreateSession(conn, tx, session);

                // 5) Invoice (force correct OwnerId)
                invoice.OwnerId = ownerId;
                var total = (invoice.Lines ?? Enumerable.Empty<CreateInvoiceLineDto>()).Sum(x => x.Amount);

                var invoiceId = _invoices.CreateInvoice(conn, tx, owner.Id, total, status: "Draft");

                foreach (var line in invoice.Lines ?? Enumerable.Empty<CreateInvoiceLineDto>())
                {
                    _invoices.AddInvoiceLine(conn, tx, invoiceId, line.Description, line.Amount);
                }

                tx.Commit();

                return new FlowResult
                {
                    OwnerId = owner.Id,
                    DogId = dog.Id,
                    BookingId = bookingId,
                    SessionId = sessionId,
                    InvoiceId = invoiceId
                };
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}

public class FlowResult
{
    public int OwnerId { get; set; }
    public int DogId { get; set; }
    public int BookingId { get; set; }
    public int SessionId { get; set; }
    public int InvoiceId { get; set; }
}