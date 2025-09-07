using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Queries.UserManagement
{
    public class SearchUsersQuery : IRequest<SearchUsersResponse>
    {
        public string SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortOrder { get; set; } = "desc";
    }
}