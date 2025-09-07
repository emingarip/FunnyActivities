using System.Collections.Generic;

namespace FunnyActivities.Application.DTOs.UserManagement
{
    public class SearchUsersResponse
    {
        public IEnumerable<UserDto> Users { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}