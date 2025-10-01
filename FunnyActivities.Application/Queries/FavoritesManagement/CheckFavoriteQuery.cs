using MediatR;
using FunnyActivities.Application.DTOs.FavoritesManagement;

namespace FunnyActivities.Application.Queries.FavoritesManagement
{
    public class CheckFavoriteQuery : IRequest<CheckFavoriteResponse>
    {
        public Guid ActivityId { get; set; }
        public Guid UserId { get; set; }
    }
}