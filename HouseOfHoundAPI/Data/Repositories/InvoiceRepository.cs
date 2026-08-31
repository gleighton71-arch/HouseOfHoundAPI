using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HouseOfHoundAPI.Models.Invoice;

public class InvoiceRepository
{
    private string _connectionString;

    public InvoiceRepository()
    {
        _connectionString = Db.GetConnectionString();
    }
    public int CreateInvoice(SqlConnection conn, SqlTransaction tx, int ownerId, decimal total, string status)
    {
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Invoices (OwnerId, TotalAmount, Status)
OUTPUT INSERTED.InvoiceId
VALUES (@OwnerId, @TotalAmount, @Status);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            cmd.Parameters.AddWithValue("@TotalAmount", total);
            cmd.Parameters.AddWithValue("@Status", status);

            return (int)cmd.ExecuteScalar();
        }
    }

    public void AddInvoiceLine(SqlConnection conn, SqlTransaction tx, int invoiceId, string description, decimal amount)
    {
        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.InvoiceLines (InvoiceId, Description, Amount)
VALUES (@InvoiceId, @Description, @Amount);", conn, tx))
        {
            cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@Amount", amount);

            cmd.ExecuteNonQuery();
        }
    }

    public void SavePaymentLink(int invoiceId, string stripeSessionId, string stripeUrl, string twilioMessageSid, bool whatsappSent, bool emailSent)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
UPDATE dbo.Invoices
SET StripeCheckoutSessionId = @StripeCheckoutSessionId,
    StripeCheckoutUrl = @StripeCheckoutUrl,
    StripePaymentStatus = @StripePaymentStatus,
    Status = 'Sent',
    PaymentSentUtc = SYSUTCDATETIME(),
    TwilioMessageSid = @TwilioMessageSid,
EmailSentUtc = CASE WHEN @EmailSent = 1 THEN getdate() ELSE null END
WHERE InvoiceId = @InvoiceId;", conn))
        {
            cmd.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
            cmd.Parameters.Add("@StripeCheckoutSessionId", SqlDbType.NVarChar, 200).Value = stripeSessionId;
            cmd.Parameters.Add("@StripeCheckoutUrl", SqlDbType.NVarChar, 500).Value = stripeUrl;
            cmd.Parameters.Add("@StripePaymentStatus", SqlDbType.NVarChar, 50).Value = "Pending";
            cmd.Parameters.Add("@TwilioMessageSid", SqlDbType.NVarChar, 100).Value =
                (object)twilioMessageSid ?? DBNull.Value;
            cmd.Parameters.Add("@EmailSent", SqlDbType.Bit).Value = emailSent;
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public void MarkPaid(int invoiceId,string CheckOutSessionId)
    {
        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
UPDATE dbo.Invoices
SET StripePaymentStatus = 'Paid',
    Status = 'Paid',
    PaidUtc = SYSUTCDATETIME(),
CheckoutSessionId = @CheckoutSessionId
WHERE InvoiceId = @InvoiceId;", conn))
        {
            cmd.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId;
            cmd.Parameters.Add("@CheckoutSessionId", SqlDbType.NVarChar).Value = CheckOutSessionId;
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public List<InvoiceSummaryDto> GetInvoicesForOwner(int ownerId, int? dogId = null, DateTime? fromDate = null, DateTime? toDate = null, string dateType = "invoice", string status = null, string search = null)
    {
        var invoices = new List<InvoiceSummaryDto>();
        var invoiceLookup = new Dictionary<int, InvoiceSummaryDto>();
        var useBookingDate = string.Equals(dateType, "booking", StringComparison.OrdinalIgnoreCase);

        using (var conn = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(@"
SELECT
    i.InvoiceId,
    i.CreatedUtc AS InvoiceDate,
    b.StartTimeUtc AS BookingDate,
    i.TotalAmount,
    i.Status,
    i.StripeCheckoutUrl,
    b.BookingId,
    d.DogId,
    d.Name AS DogName,
    il.InvoiceLineId,
    il.Description,
    il.Amount AS LineAmount
FROM dbo.Invoices i
LEFT JOIN dbo.Bookings b ON b.InvoiceId = i.InvoiceId
LEFT JOIN dbo.Dogs d ON d.DogId = b.DogId
LEFT JOIN dbo.InvoiceLines il ON il.InvoiceId = i.InvoiceId
WHERE i.OwnerId = @OwnerId
  AND (@DogId IS NULL OR d.DogId = @DogId)
  AND (@Status IS NULL OR i.Status = @Status)
  AND (@FromDate IS NULL OR ((@UseBookingDate = 1 AND b.StartTimeUtc >= @FromDate) OR (@UseBookingDate = 0 AND i.CreatedUtc >= @FromDate)))
  AND (@ToDateExclusive IS NULL OR ((@UseBookingDate = 1 AND b.StartTimeUtc < @ToDateExclusive) OR (@UseBookingDate = 0 AND i.CreatedUtc < @ToDateExclusive)))
  AND (@Search IS NULL OR EXISTS (
      SELECT 1
      FROM dbo.InvoiceLines searchLine
      WHERE searchLine.InvoiceId = i.InvoiceId
        AND searchLine.Description LIKE @SearchLike
  ))
ORDER BY i.CreatedUtc DESC, i.InvoiceId DESC, il.InvoiceLineId ASC;", conn))
        {
            cmd.Parameters.Add("@OwnerId", SqlDbType.Int).Value = ownerId;
            cmd.Parameters.Add("@DogId", SqlDbType.Int).Value = (object)dogId ?? DBNull.Value;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(status) ? (object)DBNull.Value : status.Trim();
            cmd.Parameters.Add("@UseBookingDate", SqlDbType.Bit).Value = useBookingDate;
            cmd.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object)fromDate ?? DBNull.Value;
            cmd.Parameters.Add("@ToDateExclusive", SqlDbType.DateTime2).Value = (object)(toDate?.Date.AddDays(1)) ?? DBNull.Value;
            cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search.Trim();
            cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 204).Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : "%" + search.Trim() + "%";

            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var invoiceId = (int)rdr["InvoiceId"];

                    InvoiceSummaryDto invoice;
                    if (!invoiceLookup.TryGetValue(invoiceId, out invoice))
                    {
                        invoice = new InvoiceSummaryDto
                        {
                            InvoiceId = invoiceId,
                            InvoiceDate = (DateTime)rdr["InvoiceDate"],
                            BookingDate = rdr["BookingDate"] == DBNull.Value ? (DateTime?)null : (DateTime)rdr["BookingDate"],
                            TotalAmount = (decimal)rdr["TotalAmount"],
                            Status = rdr["Status"]?.ToString(),
                            StripeCheckoutUrl = rdr["StripeCheckoutUrl"]?.ToString(),
                            BookingId = rdr["BookingId"] == DBNull.Value ? (int?)null : (int)rdr["BookingId"],
                            DogId = rdr["DogId"] == DBNull.Value ? (int?)null : (int)rdr["DogId"],
                            DogName = rdr["DogName"]?.ToString()
                        };

                        invoiceLookup.Add(invoiceId, invoice);
                        invoices.Add(invoice);
                    }

                    if (rdr["InvoiceLineId"] != DBNull.Value)
                    {
                        invoice.Lines.Add(new InvoiceLine
                        {
                            InvoiceLineId = (int)rdr["InvoiceLineId"],
                            InvoiceId = invoiceId,
                            Description = rdr["Description"]?.ToString(),
                            Amount = (decimal)rdr["LineAmount"]
                        });
                    }
                }
            }
        }

        return invoices;
    }
}
