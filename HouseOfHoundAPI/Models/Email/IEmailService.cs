using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfHoundAPI.Models.Email
{
    public interface IEmailService
    {
        Task SendPaymentLinkAsync(string email, string name, decimal amount, string paymentUrl);
    }
  
}
