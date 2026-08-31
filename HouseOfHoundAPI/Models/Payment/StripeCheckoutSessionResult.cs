namespace HouseOfHoundAPI.Models.Payment
{
    public class StripeCheckoutSessionResult
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
    }
}