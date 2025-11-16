using MediatR;
using FunnyActivities.Application.DTOs.Shared;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Queries.PersonaManagement
{
    public class GetPersonasQuery : IRequest<PagedResult<PersonaDto>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }
}