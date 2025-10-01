using MediatR;
using FunnyActivities.Application.DTOs.FavoritesManagement;

namespace FunnyActivities.Application.Queries.FavoritesManagement
{
    public class GetUserFavoritesQuery : IRequest<IEnumerable<FavoritesDto>>
    {
        public Guid UserId { get; set; }
    }
}