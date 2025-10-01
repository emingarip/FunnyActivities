using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.UserManagement;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UserManagement
{
    public class GetUserCountQueryHandler : IRequestHandler<GetUserCountQuery, int>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserCountQueryHandler> _logger;

        public GetUserCountQueryHandler(IUserRepository userRepository, ILogger<GetUserCountQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<int> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[USER-COUNT-HANDLER] Starting GetUserCountQueryHandler");

            var startTime = DateTime.UtcNow;

            try
            {
                var count = await _userRepository.GetTotalCountAsync().ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-COUNT-HANDLER] GetUserCountQueryHandler completed in {Duration}ms. Total users: {Count}",
                    duration.TotalMilliseconds, count);

                return count;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-COUNT-HANDLER] GetUserCountQueryHandler failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }
    }
}