using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.FavoritesManagement;
using FunnyActivities.Application.DTOs.FavoritesManagement;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.FavoritesManagement
{
    public class AddToFavoritesCommandHandler : IRequestHandler<AddToFavoritesCommand, FavoritesDto>
    {
        private readonly IFavoritesRepository _favoritesRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AddToFavoritesCommandHandler> _logger;

        public AddToFavoritesCommandHandler(
            IFavoritesRepository favoritesRepository,
            IActivityRepository activityRepository,
            IUserRepository userRepository,
            ILogger<AddToFavoritesCommandHandler> logger)
        {
            _favoritesRepository = favoritesRepository;
            _activityRepository = activityRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<FavoritesDto> Handle(AddToFavoritesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding activity {ActivityId} to favorites for user {UserId}", request.ActivityId, request.UserId);

            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId);
                throw new KeyNotFoundException("User not found");
            }

            // Verify activity exists
            var activity = await _activityRepository.GetByIdAsync(request.ActivityId);
            if (activity == null)
            {
                _logger.LogWarning("Activity {ActivityId} not found", request.ActivityId);
                throw new KeyNotFoundException("Activity not found");
            }

            // Check if already favorited
            var existingFavorite = await _favoritesRepository.GetByUserAndActivityAsync(request.UserId, request.ActivityId);
            if (existingFavorite != null)
            {
                _logger.LogWarning("Activity {ActivityId} is already in favorites for user {UserId}", request.ActivityId, request.UserId);
                throw new InvalidOperationException("Activity is already in favorites");
            }

            // Create new favorite
            var favorite = Favorites.Create(request.UserId, request.ActivityId);
            await _favoritesRepository.AddAsync(favorite);

            _logger.LogInformation("Successfully added activity {ActivityId} to favorites for user {UserId}", request.ActivityId, request.UserId);

            return new FavoritesDto
            {
                Id = favorite.Id,
                UserId = favorite.UserId,
                ActivityId = favorite.ActivityId,
                CreatedAt = favorite.CreatedAt,
                UpdatedAt = favorite.UpdatedAt
            };
        }
    }
}