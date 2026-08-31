namespace HouseOfHoundAPI.Models.Payment
{
    public class SendInvoicePaymentLinkRequest
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public string WhatsAppNumber { get; set; }   // E.164, e.g. +447900123456
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
    }
}