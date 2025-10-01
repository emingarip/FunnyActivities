using MediatR;

namespace FunnyActivities.Application.Commands.FavoritesManagement
{
    public class RemoveFromFavoritesCommand : IRequest
    {
        public Guid ActivityId { get; set; }
        public Guid UserId { get; set; }
    }
}