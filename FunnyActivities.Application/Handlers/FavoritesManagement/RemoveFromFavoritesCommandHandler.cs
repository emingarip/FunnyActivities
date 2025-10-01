using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FunnyActivities.Application.Commands.FavoritesManagement;
using FunnyActivities.Application.Interfaces;

namespace FunnyActivities.Application.Handlers.FavoritesManagement
{
    public class RemoveFromFavoritesCommandHandler : IRequestHandler<RemoveFromFavoritesCommand>
    {
        private readonly IFavoritesRepository _favoritesRepository;
        private readonly ILogger<RemoveFromFavoritesCommandHandler> _logger;

        public RemoveFromFavoritesCommandHandler(
            IFavoritesRepository favoritesRepository,
            ILogger<RemoveFromFavoritesCommandHandler> logger)
        {
            _favoritesRepository = favoritesRepository;
            _logger = logger;
        }

        public async Task Handle(RemoveFromFavoritesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Removing activity {ActivityId} from favorites for user {UserId}", request.ActivityId, request.UserId);

            // Get the favorite entry
            var favorite = await _favoritesRepository.GetByUserAndActivityAsync(request.UserId, request.ActivityId);
            if (favorite == null)
            {
                _logger.LogWarning("Favorite not found for activity {ActivityId} and user {UserId}", request.ActivityId, request.UserId);
                throw new KeyNotFoundException("Activity is not in favorites");
            }

            // Remove the favorite
            await _favoritesRepository.DeleteAsync(favorite);

            _logger.LogInformation("Successfully removed activity {ActivityId} from favorites for user {UserId}", request.ActivityId, request.UserId);
        }
    }
}