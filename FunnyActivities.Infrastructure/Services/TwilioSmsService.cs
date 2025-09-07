using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Infrastructure.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;

        public TwilioSmsService(string accountSid, string authToken, string fromNumber)
        {
            _accountSid = accountSid;
            _authToken = authToken;
            _fromNumber = fromNumber;
        }

        public async Task SendSmsAsync(string to, string message)
        {
            TwilioClient.Init(_accountSid, _authToken);
            await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromNumber),
                to: new Twilio.Types.PhoneNumber(to)
            );
        }
    }
}