using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FunnyActivities.Application.Commands.NotificationSystem;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers
{
    public class SendAlertSmsHandler : IRequestHandler<SendAlertSmsCommand>
    {
        private readonly ISmsService _smsService;

        public SendAlertSmsHandler(ISmsService smsService)
        {
            _smsService = smsService;
        }

        public async Task Handle(SendAlertSmsCommand request, CancellationToken cancellationToken)
        {
            await _smsService.SendSmsAsync(request.PhoneNumber, request.Message);
        }
    }
}