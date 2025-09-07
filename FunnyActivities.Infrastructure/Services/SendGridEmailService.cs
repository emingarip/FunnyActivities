using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;

        public SendGridEmailService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress("noreply@yourapp.com", "Your App");
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
            await client.SendEmailAsync(msg);
        }
    }
}