using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Application.Queries.UserManagement;
using FunnyActivities.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FunnyActivities.Application.Handlers.UserManagement
{
    public class GetUserGrowthQueryHandler : IRequestHandler<GetUserGrowthQuery, List<UserGrowthDataPoint>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserGrowthQueryHandler> _logger;

        public GetUserGrowthQueryHandler(IUserRepository userRepository, ILogger<GetUserGrowthQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<UserGrowthDataPoint>> Handle(GetUserGrowthQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[USER-GROWTH-HANDLER] Starting GetUserGrowthQueryHandler for period: {Period}", request.Period);

            var startTime = DateTime.UtcNow;

            try
            {
                // Calculate days based on period
                var days = request.Period.ToLower() switch
                {
                    "weekly" => 7,
                    "monthly" => 30,
                    "quarterly" => 90,
                    _ => request.Days
                };

                var data = await _userRepository.GetUserGrowthDataAsync(request.Period, days).ConfigureAwait(false);

                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogInformation("[USER-GROWTH-HANDLER] GetUserGrowthQueryHandler completed in {Duration}ms. Data points: {Count}",
                    duration.TotalMilliseconds, data.Count);

                return data;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                _logger.LogError(ex, "[USER-GROWTH-HANDLER] GetUserGrowthQueryHandler failed after {Duration}ms. Error: {ErrorMessage}",
                    duration.TotalMilliseconds, ex.Message);

                throw;
            }
        }
    }
}