using MediatR;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.UserManagement;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UserManagement
{
    public class GetOnlineUsersCountQueryHandler : IRequestHandler<GetOnlineUsersCountQuery, int>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetOnlineUsersCountQueryHandler> _logger;

        public GetOnlineUsersCountQueryHandler(IUserRepository userRepository, ILogger<GetOnlineUsersCountQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<int> Handle(GetOnlineUsersCountQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[ONLINE-USERS-COUNT-HANDLER] Starting GetOnlineUsersCountQueryHandler with threshold: {Threshold} minutes",
                request.OnlineThreshold.TotalMinutes);

            var startTime = DateTime.UtcNow;

            try
            {
                var count = await _userRepository.GetOnlineUsersCountAsync(request.OnlineThreshold).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[ONLINE-USERS-COUNT-HANDLER] GetOnlineUsersCountQueryHandler completed in {Duration}ms. Online users: {Count}",
                    duration.TotalMilliseconds, count);

                return count;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[ONLINE-USERS-COUNT-HANDLER] GetOnlineUsersCountQueryHandler failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }
    }
}