using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunnyActivities.Infrastructure.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "FunnyActivities";
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> options, ILogger<SmtpEmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.Username : _options.FromEmail;
            var displayName = string.IsNullOrWhiteSpace(_options.FromName) ? _options.Username : _options.FromName;
            var fromAddress = new MailAddress(fromEmail, displayName);

            using var message = new MailMessage
            {
                From = fromAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(to));

            _logger.LogInformation("[SMTP] Sending email to {Recipient} via {Host}:{Port}", to, _options.Host, _options.Port);
            await client.SendMailAsync(message);
        }
    }
}
