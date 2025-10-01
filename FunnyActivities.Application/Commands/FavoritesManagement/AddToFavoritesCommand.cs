using MediatR;
using FunnyActivities.Application.DTOs.FavoritesManagement;

namespace FunnyActivities.Application.Commands.FavoritesManagement
{
    public class AddToFavoritesCommand : IRequest<FavoritesDto>
    {
        public Guid ActivityId { get; set; }
        public Guid UserId { get; set; }
    }
}