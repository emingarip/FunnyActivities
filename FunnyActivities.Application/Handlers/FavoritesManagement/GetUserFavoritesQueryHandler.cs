using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Queries.FavoritesManagement;
using FunnyActivities.Application.DTOs.FavoritesManagement;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.FavoritesManagement
{
    public class GetUserFavoritesQueryHandler : IRequestHandler<GetUserFavoritesQuery, IEnumerable<FavoritesDto>>
    {
        private readonly IFavoritesRepository _favoritesRepository;
        private readonly ILogger<GetUserFavoritesQueryHandler> _logger;

        public GetUserFavoritesQueryHandler(
            IFavoritesRepository favoritesRepository,
            ILogger<GetUserFavoritesQueryHandler> logger)
        {
            _favoritesRepository = favoritesRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<FavoritesDto>> Handle(GetUserFavoritesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting favorites for user {UserId}", request.UserId);

            var favorites = await _favoritesRepository.GetUserFavoritesAsync(request.UserId);

            var result = favorites.Select(f => new FavoritesDto
            {
                Id = f.Id,
                UserId = f.UserId,
                ActivityId = f.ActivityId,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} favorites for user {UserId}", result.Count, request.UserId);

            return result;
        }
    }
}