using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Queries.FavoritesManagement;
using FunnyActivities.Application.DTOs.FavoritesManagement;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.FavoritesManagement
{
    public class CheckFavoriteQueryHandler : IRequestHandler<CheckFavoriteQuery, CheckFavoriteResponse>
    {
        private readonly IFavoritesRepository _favoritesRepository;
        private readonly ILogger<CheckFavoriteQueryHandler> _logger;

        public CheckFavoriteQueryHandler(
            IFavoritesRepository favoritesRepository,
            ILogger<CheckFavoriteQueryHandler> logger)
        {
            _favoritesRepository = favoritesRepository;
            _logger = logger;
        }

        public async Task<CheckFavoriteResponse> Handle(CheckFavoriteQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Checking if activity {ActivityId} is favorited by user {UserId}", request.ActivityId, request.UserId);

            var isFavorited = await _favoritesRepository.ExistsAsync(request.UserId, request.ActivityId);

            _logger.LogInformation("Activity {ActivityId} is favorited by user {UserId}: {IsFavorited}", request.ActivityId, request.UserId, isFavorited);

            return new CheckFavoriteResponse
            {
                IsFavorited = isFavorited
            };
        }
    }
}